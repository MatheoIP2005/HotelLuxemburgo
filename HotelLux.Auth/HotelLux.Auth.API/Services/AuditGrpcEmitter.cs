using Grpc.Net.Client;
using HotelLux.Auth.Business.Interfaces;
using HotelLux.Protos.Audit;

namespace HotelLux.Auth.API.Services;

public class AuditGrpcEmitter : IAuditEmitter
{
    private const string ServicioOrigen = "auth-service";

    private readonly GrpcChannel _channel;
    private readonly ILogger<AuditGrpcEmitter> _logger;

    public AuditGrpcEmitter(ILogger<AuditGrpcEmitter> logger, IConfiguration configuration)
    {
        _logger = logger;
        // h2c (HTTP/2 cleartext) requiere este switch en .NET para Grpc.Net.Client.
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
                ServicioOrigen = ServicioOrigen,
                TablaAfectada = tablaAfectada,
                // Normaliza verbos de dominio (LOGIN/LOGOUT/CREATE/DISABLE/...) al enum
                // INSERT/UPDATE/DELETE que exige chk_auditoria_operacion en la BD.
                Operacion = MapOperacion(operacion),
                EntidadGuid = entidadGuid,
                IdRegistro = string.Empty,
                UsuarioGuid = usuarioGuid,
                UsuarioEjecutor = usuarioGuid,
                IpOrigen = string.Empty,
                DatosAnterioresJson = string.Empty,
                DatosNuevosJson = string.IsNullOrWhiteSpace(detalleJson)
                    ? $"{{\"verbo\":\"{operacion}\"}}"
                    : $"{{\"verbo\":\"{operacion}\",\"detalle\":{detalleJson}}}",
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

    private static string MapOperacion(string op) => (op ?? string.Empty).ToUpperInvariant() switch
    {
        "INSERT" or "CREATE" or "LOGIN" or "ASSIGN_ROLE" => "INSERT",
        "UPDATE" or "DISABLE" or "ENABLE" or "CAMBIO_PASSWORD" => "UPDATE",
        "DELETE" or "LOGOUT" or "REMOVE_ROLE" or "REVOKE" => "DELETE",
        _ => "UPDATE"
    };
}
