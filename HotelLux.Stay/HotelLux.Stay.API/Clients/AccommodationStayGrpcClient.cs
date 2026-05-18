using Grpc.Net.Client;
using HotelLux.Protos.Accommodation;
using HotelLux.Stay.Business.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HotelLux.Stay.API.Clients;

public class AccommodationStayGrpcClient : IAccommodationStayClient
{
    private readonly GrpcChannel _channel;
    private readonly ILogger<AccommodationStayGrpcClient> _logger;

    public AccommodationStayGrpcClient(IConfiguration config, ILogger<AccommodationStayGrpcClient> logger)
    {
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        var address = config["AccommodationService:GrpcAddress"] ?? "http://localhost:5102";
        var handler = new SocketsHttpHandler { EnableMultipleHttp2Connections = true };
        _channel = GrpcChannel.ForAddress(address, new GrpcChannelOptions { HttpHandler = handler });
        _logger = logger;
    }

    public async Task<bool> UpdateRoomStatusAsync(
        Guid habitacionGuid, string nuevoEstado, Guid operacionGuid, CancellationToken ct = default)
    {
        try
        {
            var client = new AccommodationService.AccommodationServiceClient(_channel);
            var reply = await client.UpdateRoomStatusAsync(new UpdateRoomStatusRequest
            {
                HabitacionGuid = habitacionGuid.ToString(),
                NuevoEstado = nuevoEstado,
                OperacionGuid = operacionGuid.ToString()
            }, cancellationToken: ct);
            return reply.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateRoomStatus gRPC error habitacion={Hab}", habitacionGuid);
            return false;
        }
    }
}
