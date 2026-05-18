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
