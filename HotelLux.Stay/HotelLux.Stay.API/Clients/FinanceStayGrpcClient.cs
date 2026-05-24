using Grpc.Net.Client;
using HotelLux.Protos.Finance;
using HotelLux.Shared.Grpc;
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
        var address = config["FinanceService:GrpcAddress"] ?? config["GrpcClients:FinanceUrl"];
        address = string.IsNullOrWhiteSpace(address)
            ? GrpcChannelFactory.ResolveAddress(config, "FinanceService:GrpcAddress", null, 5105)
            : address.Trim();
        _channel = GrpcChannelFactory.Create(address);
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
