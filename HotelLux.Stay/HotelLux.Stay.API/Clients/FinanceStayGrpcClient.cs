using Grpc.Net.Client;
using HotelLux.Protos.Finance;
using HotelLux.Stay.Business.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HotelLux.Stay.API.Clients;

public class FinanceStayGrpcClient : IFinanceStayClient
{
    private readonly GrpcChannel _channel;
    private readonly ILogger<FinanceStayGrpcClient> _logger;

    public FinanceStayGrpcClient(IConfiguration config, ILogger<FinanceStayGrpcClient> logger)
    {
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        var address = config["FinanceService:GrpcAddress"]
            ?? config["GrpcClients:FinanceUrl"]
            ?? "http://127.0.0.1:5105";
        var handler = new SocketsHttpHandler { EnableMultipleHttp2Connections = true };
        _channel = GrpcChannel.ForAddress(address, new GrpcChannelOptions { HttpHandler = handler });
        _logger = logger;
    }

    public async Task<bool> GenerateFinalInvoiceAsync(
        Guid estadiaGuid,
        Guid reservaGuid,
        Guid clienteGuid,
        Guid sucursalGuid,
        string creadoPorUsuario,
        CancellationToken ct = default)
    {
        try
        {
            var client = new FinanceService.FinanceServiceClient(_channel);

            var response = await client.GenerateFinalInvoiceAsync(new GenerateFinalInvoiceRequest
            {
                EstadiaGuid = estadiaGuid.ToString(),
                ReservaGuid = reservaGuid.ToString(),
                ClienteGuid = clienteGuid.ToString(),
                SucursalGuid = sucursalGuid.ToString(),
                CreadoPorUsuario = creadoPorUsuario
            }, cancellationToken: ct);

            if (!response.Success)
                _logger.LogWarning("Finance GenerateFinalInvoice falló estadia={Estadia}: {Mensaje}",
                    estadiaGuid, response.Mensaje);
            return response.Success;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Finance GenerateFinalInvoice error estadia={Estadia}", estadiaGuid);
            return false;
        }
    }
}
