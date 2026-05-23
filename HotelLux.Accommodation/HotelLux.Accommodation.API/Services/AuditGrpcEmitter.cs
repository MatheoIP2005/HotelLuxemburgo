using Grpc.Net.Client;
using HotelLux.Accommodation.Business.Interfaces;
using HotelLux.Protos.Audit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HotelLux.Accommodation.API.Services;

public class AuditGrpcEmitter : IAuditEmitter
{
    private readonly GrpcChannel _channel;
    private readonly ILogger<AuditGrpcEmitter> _logger;

    public AuditGrpcEmitter(IConfiguration config, ILogger<AuditGrpcEmitter> logger)
    {
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        var address = config["AuditService:GrpcAddress"] ?? "http://localhost:5108";
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
