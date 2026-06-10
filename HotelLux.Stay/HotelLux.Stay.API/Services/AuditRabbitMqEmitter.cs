using HotelLux.Shared.Events;
using HotelLux.Stay.Business.Interfaces;
using MassTransit;

namespace HotelLux.Stay.API.Services;

public class AuditRabbitMqEmitter : IAuditEmitter
{
    private readonly IBus _bus;
    private readonly ILogger<AuditRabbitMqEmitter> _logger;

    public AuditRabbitMqEmitter(IBus bus, ILogger<AuditRabbitMqEmitter> logger)
    {
        _bus = bus;
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
                await _bus.Publish(new AuditEventMessage
                {
                    ServicioOrigen = servicioOrigen,
                    TablaAfectada = tablaAfectada,
                    Operacion = operacion,
                    EntidadGuid = entidadGuid,
                    IdRegistro = idRegistro,
                    UsuarioGuid = usuarioGuid,
                    UsuarioEjecutor = usuarioEjecutor,
                    IpOrigen = ipOrigen,
                    DatosAnterioresJson = datosAnterioresJson,
                    DatosNuevosJson = datosNuevosJson,
                    FechaEventoUtc = DateTimeOffset.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Audit] No se pudo emitir evento");
            }
        });
    }
}
