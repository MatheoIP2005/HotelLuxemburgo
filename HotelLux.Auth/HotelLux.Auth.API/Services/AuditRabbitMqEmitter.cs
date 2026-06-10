using HotelLux.Auth.Business.Interfaces;
using HotelLux.Shared.Events;
using MassTransit;

namespace HotelLux.Auth.API.Services;

public class AuditRabbitMqEmitter : IAuditEmitter
{
    private const string ServicioOrigen = "auth-service";

    private readonly IBus _bus;
    private readonly ILogger<AuditRabbitMqEmitter> _logger;

    public AuditRabbitMqEmitter(IBus bus, ILogger<AuditRabbitMqEmitter> logger)
    {
        _bus = bus;
        _logger = logger;
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
            await _bus.Publish(new AuditEventMessage
            {
                ServicioOrigen = ServicioOrigen,
                TablaAfectada = tablaAfectada,
                Operacion = MapOperacion(operacion),
                EntidadGuid = entidadGuid,
                UsuarioGuid = usuarioGuid,
                UsuarioEjecutor = usuarioGuid,
                DatosNuevosJson = string.IsNullOrWhiteSpace(detalleJson)
                    ? $"{{\"verbo\":\"{operacion}\"}}"
                    : $"{{\"verbo\":\"{operacion}\",\"detalle\":{detalleJson}}}",
                FechaEventoUtc = DateTimeOffset.UtcNow
            }, cancellationToken);
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
