using HotelLux.Audit.DataAccess.Context;
using HotelLux.Audit.DataAccess.Entities;
using HotelLux.Shared.Events;
using MassTransit;

namespace HotelLux.Audit.API.Consumers;

public class AuditEventConsumer : IConsumer<AuditEventMessage>
{
    private readonly AuditDbContext _db;
    private readonly ILogger<AuditEventConsumer> _logger;

    public AuditEventConsumer(AuditDbContext db, ILogger<AuditEventConsumer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AuditEventMessage> context)
    {
        try
        {
            var message = context.Message;
            var entity = new EventoAuditoriaEntity
            {
                AuditoriaGuid = message.EventId != Guid.Empty ? message.EventId : Guid.NewGuid(),
                TablaAfectada = message.TablaAfectada,
                Operacion = NormalizeOperacion(message.Operacion),
                EntidadGuid = Guid.TryParse(message.EntidadGuid, out var entidadGuid) ? entidadGuid : null,
                IdRegistroAfectado = string.IsNullOrWhiteSpace(message.IdRegistro) ? null : message.IdRegistro,
                DatosAnteriores = string.IsNullOrWhiteSpace(message.DatosAnterioresJson) ? null : message.DatosAnterioresJson,
                DatosNuevos = string.IsNullOrWhiteSpace(message.DatosNuevosJson) ? null : message.DatosNuevosJson,
                UsuarioEjecutor = string.IsNullOrWhiteSpace(message.UsuarioEjecutor) ? "system" : message.UsuarioEjecutor,
                UsuarioGuid = Guid.TryParse(message.UsuarioGuid, out var usuarioGuid) ? usuarioGuid : null,
                IpOrigen = string.IsNullOrWhiteSpace(message.IpOrigen) ? null : message.IpOrigen,
                ServicioOrigen = message.ServicioOrigen,
                FechaEventoUtc = message.FechaEventoUtc,
                Activo = true
            };

            await _db.EventosAuditoria.AddAsync(entity, context.CancellationToken);
            await _db.SaveChangesAsync(context.CancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo persistir evento de auditoría desde RabbitMQ");
            throw;
        }
    }

    private static string NormalizeOperacion(string op) => (op ?? string.Empty).ToUpperInvariant() switch
    {
        "INSERT" or "CREATE" or "LOGIN" or "ASSIGN_ROLE" => "INSERT",
        "DELETE" or "LOGOUT" or "REMOVE_ROLE" or "REVOKE" => "DELETE",
        _ => "UPDATE"
    };
}
