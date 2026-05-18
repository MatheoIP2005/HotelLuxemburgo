using Grpc.Net.Client;
using HotelLux.Finance.Business.Interfaces;
using HotelLux.Protos.Audit;

namespace HotelLux.Finance.API.Clients;

public class AuditGrpcClient : IAuditEmitter
{
    private readonly GrpcChannel _channel;
    private readonly ILogger<AuditGrpcClient> _logger;

    public AuditGrpcClient(IConfiguration config, ILogger<AuditGrpcClient> logger)
    {
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        var address = config["AuditService:GrpcAddress"]
            ?? config["GrpcClients:AuditUrl"]
            ?? "http://127.0.0.1:5108";
        var handler = new SocketsHttpHandler { EnableMultipleHttp2Connections = true };
        _channel = GrpcChannel.ForAddress(address, new GrpcChannelOptions { HttpHandler = handler });
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
                    FechaEventoIso = string.Empty
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Audit] No se pudo emitir evento");
            }
        });
    }
}
