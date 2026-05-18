using HotelLux.Stay.DataAccess.Context;
using HotelLux.Stay.DataAccess.Entities;
using HotelLux.Stay.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelLux.Stay.DataAccess.Repositories;

public class EstadiaRepository : IEstadiaRepository
{
    private readonly StayDbContext _db;
    public EstadiaRepository(StayDbContext db) => _db = db;

    public async Task<EstadiaEntity?> ObtenerPorGuidAsync(Guid estadiaGuid, CancellationToken ct = default)
        => await _db.Estadias.AsNoTracking()
            .FirstOrDefaultAsync(x => x.EstadiaGuid == estadiaGuid && !x.EsEliminado, ct);

    public async Task<EstadiaEntity?> ObtenerPorIdAsync(int idEstadia, CancellationToken ct = default)
        => await _db.Estadias.AsNoTracking()
            .FirstOrDefaultAsync(x => x.IdEstadia == idEstadia && !x.EsEliminado, ct);

    public async Task<EstadiaEntity?> ObtenerParaActualizarAsync(Guid estadiaGuid, CancellationToken ct = default)
        => await _db.Estadias
            .FirstOrDefaultAsync(x => x.EstadiaGuid == estadiaGuid && !x.EsEliminado, ct);

    public async Task<EstadiaEntity?> ObtenerActivaPorReservaGuidAsync(Guid reservaGuid, CancellationToken ct = default)
        => await _db.Estadias.AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.ReservaGuid == reservaGuid && x.Estado == "ACT" && !x.EsEliminado, ct);

    public async Task<EstadiaEntity?> ObtenerActivaPorReservaHabitacionGuidAsync(Guid reservaHabitacionGuid, CancellationToken ct = default)
        => await _db.Estadias.AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.ReservaHabitacionGuid == reservaHabitacionGuid && x.Estado == "ACT" && !x.EsEliminado, ct);

    public async Task<(IReadOnlyList<EstadiaEntity> Items, int Total)> ListarAsync(
        string? estado, Guid? sucursalGuid, int pagina, int limite, CancellationToken ct = default)
    {
        var q = _db.Estadias.AsNoTracking().Where(x => !x.EsEliminado);
        if (!string.IsNullOrWhiteSpace(estado))
        {
            var e = estado.Trim().ToUpperInvariant();
            q = q.Where(x => x.Estado == e);
        }

        if (sucursalGuid.HasValue)
            q = q.Where(x => x.SucursalGuid == sucursalGuid.Value);

        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(x => x.FechaRegistroUtc)
            .Skip((pagina - 1) * limite)
            .Take(limite)
            .ToListAsync(ct);
        return (items, total);
    }

    public async Task AgregarAsync(EstadiaEntity entity, CancellationToken ct = default)
        => await _db.Estadias.AddAsync(entity, ct);

    public void Actualizar(EstadiaEntity entity) => _db.Estadias.Update(entity);
}
