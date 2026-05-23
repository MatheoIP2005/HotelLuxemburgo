using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using HotelLux.Protos.Accommodation;
using HotelLux.Reservation.Business.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HotelLux.Reservation.API.Clients;

public class AccommodationGrpcClient : IAccommodationClient
{
    private readonly GrpcChannel _channel;
    private readonly ILogger<AccommodationGrpcClient> _logger;

    public AccommodationGrpcClient(IConfiguration config, ILogger<AccommodationGrpcClient> logger)
    {
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        var address = config["AccommodationService:GrpcAddress"] ?? "http://localhost:5102";
        var handler = new SocketsHttpHandler { EnableMultipleHttp2Connections = true };
        _channel = GrpcChannel.ForAddress(address, new GrpcChannelOptions { HttpHandler = handler });
        _logger = logger;
    }

    public async Task<IReadOnlyList<HabitacionDisponibleInfo>> ListarDisponiblesAsync(
        Guid sucursalGuid,
        DateOnly fechaInicio,
        DateOnly fechaFin,
        Guid? tipoHabitacionGuid = null,
        int cantidadPersonas = 0,
        CancellationToken ct = default)
    {
        try
        {
            var client = new AccommodationService.AccommodationServiceClient(_channel);
            var request = new CheckAvailabilityRequest
            {
                SucursalGuid = sucursalGuid.ToString(),
                FechaEntrada = ToUtcTimestamp(fechaInicio),
                FechaSalida = ToUtcTimestamp(fechaFin),
                CantidadPersonas = cantidadPersonas
            };

            if (tipoHabitacionGuid.HasValue && tipoHabitacionGuid.Value != Guid.Empty)
                request.TipoHabitacionGuid = tipoHabitacionGuid.Value.ToString();

            var response = await client.CheckAvailabilityAsync(request, cancellationToken: ct);
            if (!response.Disponible || response.Habitaciones.Count == 0)
                return Array.Empty<HabitacionDisponibleInfo>();

            return response.Habitaciones
                .Where(h => Guid.TryParse(h.HabitacionGuid, out _))
                .Select(h => new HabitacionDisponibleInfo(
                    Guid.Parse(h.HabitacionGuid),
                    Guid.TryParse(h.TipoHabitacionGuid, out var tipo) ? tipo : Guid.Empty,
                    (decimal)h.PrecioNoche))
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "CheckAvailability error sucursal={Sucursal} tipo={Tipo}",
                sucursalGuid, tipoHabitacionGuid);
            return Array.Empty<HabitacionDisponibleInfo>();
        }
    }

    public async Task<HabitacionDisponibleInfo?> ResolverPorTipoAsync(
        Guid sucursalGuid,
        Guid tipoHabitacionGuid,
        DateOnly fechaInicio,
        DateOnly fechaFin,
        CancellationToken ct = default)
    {
        try
        {
            var client = new AccommodationService.AccommodationServiceClient(_channel);
            var response = await client.FindAvailableRoomByTypeAsync(new FindAvailableRoomByTypeRequest
            {
                SucursalGuid = sucursalGuid.ToString(),
                TipoHabitacionGuid = tipoHabitacionGuid.ToString(),
                FechaInicio = ToUtcTimestamp(fechaInicio),
                FechaFin = ToUtcTimestamp(fechaFin)
            }, cancellationToken: ct);

            if (!response.Encontrada || !Guid.TryParse(response.HabitacionGuid, out var habitacionGuid))
                return null;

            return new HabitacionDisponibleInfo(
                habitacionGuid,
                tipoHabitacionGuid,
                (decimal)response.PrecioNoche);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "FindAvailableRoomByType error sucursal={Sucursal} tipo={Tipo}",
                sucursalGuid, tipoHabitacionGuid);
            return null;
        }
    }

    public async Task<bool> ConfirmRoomLockAsync(
        Guid habitacionGuid, Guid reservaGuid,
        DateOnly fechaInicio, DateOnly fechaFin,
        CancellationToken ct = default)
    {
        try
        {
            var client = new AccommodationService.AccommodationServiceClient(_channel);

            var response = await client.ConfirmRoomLockAsync(new ConfirmRoomLockRequest
            {
                HabitacionGuid = habitacionGuid.ToString(),
                ReservaGuid = reservaGuid.ToString(),
                FechaEntrada = ToUtcTimestamp(fechaInicio),
                FechaSalida = ToUtcTimestamp(fechaFin)
            }, cancellationToken: ct);

            if (!response.Success)
                _logger.LogWarning(
                    "ConfirmRoomLock falló habitacion={Hab} reserva={Res}: {Msg}",
                    habitacionGuid, reservaGuid, response.Mensaje);

            return response.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ConfirmRoomLock error habitacion={Hab} reserva={Res}", habitacionGuid, reservaGuid);
            return false;
        }
    }

    public async Task<bool> ReleaseRoomLockAsync(Guid habitacionGuid, Guid reservaGuid, CancellationToken ct = default)
    {
        try
        {
            var client = new AccommodationService.AccommodationServiceClient(_channel);

            var response = await client.ReleaseRoomLockAsync(new ReleaseRoomLockRequest
            {
                HabitacionGuid = habitacionGuid.ToString(),
                ReservaGuid = reservaGuid.ToString()
            }, cancellationToken: ct);

            return response.Success;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "ReleaseRoomLock error habitacion={Hab} reserva={Res}", habitacionGuid, reservaGuid);
            return false;
        }
    }

    private static Timestamp ToUtcTimestamp(DateOnly date)
    {
        var dt = DateTime.SpecifyKind(date.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        return Timestamp.FromDateTime(dt);
    }
}
