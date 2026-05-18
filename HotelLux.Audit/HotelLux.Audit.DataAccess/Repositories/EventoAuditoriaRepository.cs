using HotelLux.Audit.DataAccess.Context;
using HotelLux.Audit.DataAccess.Entities;
using HotelLux.Audit.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelLux.Audit.DataAccess.Repositories;

public class EventoAuditoriaRepository : IEventoAuditoriaRepository
{
    private readonly AuditDbContext _db;
    public EventoAuditoriaRepository(AuditDbContext db) => _db = db;

    public async Task AgregarAsync(EventoAuditoriaEntity entity, CancellationToken ct = default)
        => await _db.EventosAuditoria.AddAsync(entity, ct);

    public async Task<IReadOnlyList<EventoAuditoriaEntity>> ListarAsync(
        string? servicioOrigen,
        string? tablaAfectada,
        Guid? entidadGuid,
        string? usuarioEjecutor,
        int pagina,
        int limite,
        CancellationToken ct = default)
    {
        var q = _db.EventosAuditoria.AsNoTracking().Where(x => x.Activo);
        if (!string.IsNullOrWhiteSpace(servicioOrigen))
            q = q.Where(x => x.ServicioOrigen == servicioOrigen);
        if (!string.IsNullOrWhiteSpace(tablaAfectada))
            q = q.Where(x => x.TablaAfectada == tablaAfectada);
        if (entidadGuid.HasValue)
            q = q.Where(x => x.EntidadGuid == entidadGuid.Value);
        if (!string.IsNullOrWhiteSpace(usuarioEjecutor))
            q = q.Where(x => x.UsuarioEjecutor == usuarioEjecutor);

        return await q.OrderByDescending(x => x.FechaEventoUtc)
            .Skip((pagina - 1) * limite)
            .Take(limite)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<EventoAuditoriaEntity>> ListarPorEntidadAsync(
        Guid entidadGuid, CancellationToken ct = default)
        => await _db.EventosAuditoria.AsNoTracking()
            .Where(x => x.EntidadGuid == entidadGuid && x.Activo)
            .OrderByDescending(x => x.FechaEventoUtc)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<EventoAuditoriaEntity>> ListarPorServicioAsync(
        string servicioOrigen, CancellationToken ct = default)
        => await _db.EventosAuditoria.AsNoTracking()
            .Where(x => x.ServicioOrigen == servicioOrigen && x.Activo)
            .OrderByDescending(x => x.FechaEventoUtc)
            .ToListAsync(ct);

    public async Task<EventoAuditoriaEntity?> ObtenerPorAuditoriaGuidAsync(
        Guid auditoriaGuid, CancellationToken ct = default)
        => await _db.EventosAuditoria.AsNoTracking()
            .FirstOrDefaultAsync(x => x.AuditoriaGuid == auditoriaGuid && x.Activo, ct);
}
