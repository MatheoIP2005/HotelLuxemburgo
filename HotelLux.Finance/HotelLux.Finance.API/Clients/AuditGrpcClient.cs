using Grpc.Net.Client;
using HotelLux.Finance.Business.Interfaces;
using HotelLux.Protos.Audit;
using HotelLux.Shared.Grpc;

namespace HotelLux.Finance.API.Clients;

public class AuditGrpcClient : IAuditEmitter
{
    private readonly GrpcChannel _channel;
    private readonly ILogger<AuditGrpcClient> _logger;

    public AuditGrpcClient(IConfiguration config, ILogger<AuditGrpcClient> logger)
    {
        var address = config["AuditService:GrpcAddress"] ?? config["GrpcClients:AuditUrl"];
        address = string.IsNullOrWhiteSpace(address)
            ? GrpcChannelFactory.ResolveAddress(config, "AuditService:GrpcAddress", null, 5108)
            : address.Trim();
        _channel = GrpcChannelFactory.Create(address);
        _logger = logger;
    }

    public void EmitFireAndForget(string servicioOrigen, string tablaAfectada, string operacion,
        string entidadGuid, string? idRegistro, string usuarioGuid, string usuarioEjecutor,
        string? ipOrigen, string? datosAnterioresJson = null, string? datosNuevosJson = null)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                var client = new AuditService.AuditServiceClient(_channel);
                await client.EmitAuditEventAsync(new EmitAuditEventRequest
                {
                    ServicioOrigen = servicioOrigen,
                    TablaAfectada = tablaAfectada,
                    Operacion = operacion,
                    EntidadGuid = entidadGuid,
                    IdRegistro = idRegistro ?? string.Empty,
                    UsuarioGuid = usuarioGuid,
                    UsuarioEjecutor = usuarioEjecutor,
                    IpOrigen = ipOrigen ?? string.Empty,
                    DatosAnterioresJson = datosAnterioresJson ?? string.Empty,
                    DatosNuevosJson = datosNuevosJson ?? string.Empty,
                    FechaEventoIso = DateTimeOffset.UtcNow.ToString("o")
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Audit] No se pudo emitir evento");
            }
        });
    }
}
