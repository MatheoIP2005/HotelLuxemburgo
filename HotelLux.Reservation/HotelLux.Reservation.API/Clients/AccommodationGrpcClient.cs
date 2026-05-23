using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using HotelLux.Protos.Accommodation;
using HotelLux.Reservation.Business.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace HotelLux.Reservation.API.Clients;

public class AccommodationGrpcClient : IAccommodationClient
{
    private readonly GrpcChannel _channel;
    private readonly HttpClient? _restClient;
    private readonly ILogger<AccommodationGrpcClient> _logger;

    public AccommodationGrpcClient(IConfiguration config, ILogger<AccommodationGrpcClient> logger)
    {
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        var address = config["AccommodationService:GrpcAddress"] ?? "http://localhost:5102";
        var handler = new SocketsHttpHandler { EnableMultipleHttp2Connections = true };
        _channel = GrpcChannel.ForAddress(address, new GrpcChannelOptions { HttpHandler = handler });
        _restClient = CreateRestClient(config, address);
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
                "CheckAvailability gRPC error sucursal={Sucursal} tipo={Tipo}. Intentando fallback REST.",
                sucursalGuid, tipoHabitacionGuid);
            return await ListarDisponiblesRestAsync(
                sucursalGuid,
                fechaInicio,
                fechaFin,
                tipoHabitacionGuid,
                cantidadPersonas,
                ct);
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
                "FindAvailableRoomByType gRPC error sucursal={Sucursal} tipo={Tipo}. Intentando fallback REST.",
                sucursalGuid, tipoHabitacionGuid);
            var disponibles = await ListarDisponiblesRestAsync(
                sucursalGuid,
                fechaInicio,
                fechaFin,
                tipoHabitacionGuid,
                0,
                ct);
            return disponibles.FirstOrDefault();
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

    private static HttpClient? CreateRestClient(IConfiguration config, string grpcAddress)
    {
        var restAddress = config["AccommodationService:RestAddress"]
            ?? config["AccommodationService:HttpAddress"];

        if (string.IsNullOrWhiteSpace(restAddress) &&
            Uri.TryCreate(grpcAddress, UriKind.Absolute, out var grpcUri) &&
            grpcUri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            restAddress = grpcAddress;
        }

        if (string.IsNullOrWhiteSpace(restAddress) ||
            !Uri.TryCreate(restAddress, UriKind.Absolute, out var restUri))
        {
            return null;
        }

        return new HttpClient
        {
            BaseAddress = restUri.AbsoluteUri.EndsWith("/")
                ? restUri
                : new Uri(restUri.AbsoluteUri + "/")
        };
    }

    private async Task<IReadOnlyList<HabitacionDisponibleInfo>> ListarDisponiblesRestAsync(
        Guid sucursalGuid,
        DateOnly fechaInicio,
        DateOnly fechaFin,
        Guid? tipoHabitacionGuid,
        int cantidadPersonas,
        CancellationToken ct)
    {
        if (_restClient is null)
            return Array.Empty<HabitacionDisponibleInfo>();

        try
        {
            var query = $"api/v1/public/sucursales/{sucursalGuid}/habitaciones" +
                        $"?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}";

            if (tipoHabitacionGuid.HasValue && tipoHabitacionGuid.Value != Guid.Empty)
                query += $"&tipo_habitacion_guid={tipoHabitacionGuid.Value}";

            var response = await _restClient.GetFromJsonAsync<List<HabitacionDisponibleRestDto>>(query, ct)
                ?? [];

            return response
                .Where(h => h.DisponibleEnRango)
                .Where(h => h.EstadoHabitacion == "DIS")
                .Where(h => h.HabitacionGuid != Guid.Empty)
                .Where(h => h.TipoHabitacionGuid != Guid.Empty)
                .Where(h => cantidadPersonas <= 0 || h.CapacidadAdultos >= cantidadPersonas)
                .Select(h => new HabitacionDisponibleInfo(
                    h.HabitacionGuid,
                    h.TipoHabitacionGuid,
                    h.PrecioBase > 0 ? h.PrecioBase : 0.01m))
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "CheckAvailability REST fallback error sucursal={Sucursal} tipo={Tipo}",
                sucursalGuid, tipoHabitacionGuid);
            return Array.Empty<HabitacionDisponibleInfo>();
        }
    }

    private sealed class HabitacionDisponibleRestDto
    {
        [JsonPropertyName("habitacionGuid")]
        public Guid HabitacionGuid { get; init; }

        [JsonPropertyName("tipoHabitacionGuid")]
        public Guid TipoHabitacionGuid { get; init; }

        [JsonPropertyName("capacidadAdultos")]
        public int CapacidadAdultos { get; init; }

        [JsonPropertyName("precioBase")]
        public decimal PrecioBase { get; init; }

        [JsonPropertyName("estadoHabitacion")]
        public string EstadoHabitacion { get; init; } = string.Empty;

        [JsonPropertyName("disponibleEnRango")]
        public bool DisponibleEnRango { get; init; }
    }
}
