using HotelLux.Finance.Business.Interfaces;
using HotelLux.Shared.Events;
using MassTransit;

namespace HotelLux.Finance.API.Clients;

public class AuditRabbitMqClient : IAuditEmitter
{
    private readonly IBus _bus;
    private readonly ILogger<AuditRabbitMqClient> _logger;

    public AuditRabbitMqClient(IBus bus, ILogger<AuditRabbitMqClient> logger)
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
