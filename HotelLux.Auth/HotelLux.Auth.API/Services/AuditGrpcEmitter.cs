using Grpc.Net.Client;
using HotelLux.Auth.Business.Interfaces;
using HotelLux.Protos.Audit;

namespace HotelLux.Auth.API.Services;

public class AuditGrpcEmitter : IAuditEmitter
{
    private readonly GrpcChannel _channel;
    private readonly ILogger<AuditGrpcEmitter> _logger;

    public AuditGrpcEmitter(ILogger<AuditGrpcEmitter> logger, IConfiguration configuration)
    {
        _logger = logger;
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        var address = configuration["AuditService:GrpcAddress"] ?? "http://localhost:5108";
        var handler = new SocketsHttpHandler { EnableMultipleHttp2Connections = true };
        _channel = GrpcChannel.ForAddress(address, new GrpcChannelOptions { HttpHandler = handler });
    }

    public async Task EmitAsync(
        string tablaAfectada,
        string operacion,
        string entidadGuid,
        string usuarioGuid,
        string detalleJson,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = new AuditService.AuditServiceClient(_channel);
            var request = new EmitAuditEventRequest
            {
                ServicioOrigen = "HotelLux.Auth",
                TablaAfectada = tablaAfectada,
                Operacion = operacion,
                EntidadGuid = entidadGuid,
                IdRegistro = string.Empty,
                UsuarioGuid = usuarioGuid,
                UsuarioEjecutor = string.Empty,
                IpOrigen = string.Empty,
                DatosAnterioresJson = string.Empty,
                DatosNuevosJson = detalleJson,
                FechaEventoIso = string.Empty
            };

            await client.EmitAuditEventAsync(request, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "[Audit] No se pudo emitir evento de auditoría. Operación: {Op}", operacion);
        }
    }
}
