using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using HotelLux.Audit.DataAccess.Context;
using HotelLux.Audit.DataAccess.Entities;
using HotelLux.Protos.Audit;

namespace HotelLux.Audit.API.GrpcServices;

public class AuditGrpcService : AuditService.AuditServiceBase
{
    private readonly AuditDbContext _db;
    private readonly ILogger<AuditGrpcService> _logger;

    public AuditGrpcService(AuditDbContext db, ILogger<AuditGrpcService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public override async Task<Empty> EmitAuditEvent(EmitAuditEventRequest request, ServerCallContext context)
    {
        try
        {
            var entity = new EventoAuditoriaEntity
            {
                AuditoriaGuid = Guid.NewGuid(),
                TablaAfectada = request.TablaAfectada,
                Operacion = request.Operacion,
                EntidadGuid = Guid.TryParse(request.EntidadGuid, out var entidadGuid) ? entidadGuid : null,
                IdRegistroAfectado = string.IsNullOrWhiteSpace(request.IdRegistro) ? null : request.IdRegistro,
                DatosAnteriores = string.IsNullOrWhiteSpace(request.DatosAnterioresJson) ? null : request.DatosAnterioresJson,
                DatosNuevos = string.IsNullOrWhiteSpace(request.DatosNuevosJson) ? null : request.DatosNuevosJson,
                UsuarioEjecutor = string.IsNullOrWhiteSpace(request.UsuarioEjecutor) ? "system" : request.UsuarioEjecutor,
                UsuarioGuid = Guid.TryParse(request.UsuarioGuid, out var usuarioGuid) ? usuarioGuid : null,
                IpOrigen = string.IsNullOrWhiteSpace(request.IpOrigen) ? null : request.IpOrigen,
                ServicioOrigen = request.ServicioOrigen,
                FechaEventoUtc = DateTimeOffset.TryParse(request.FechaEventoIso, out var fecha)
                    ? fecha
                    : DateTimeOffset.UtcNow,
                Activo = true
            };

            await _db.EventosAuditoria.AddAsync(entity, context.CancellationToken);
            await _db.SaveChangesAsync(context.CancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo persistir evento de auditoría");
        }

        return new Empty();
    }
}
