using HotelLux.Stay.DataAccess.Context;
using HotelLux.Stay.DataAccess.Entities;
using HotelLux.Stay.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelLux.Stay.DataAccess.Repositories;

public class ValoracionRepository : IValoracionRepository
{
    private readonly StayDbContext _db;
    public ValoracionRepository(StayDbContext db) => _db = db;

    public async Task<(IReadOnlyList<ValoracionEntity> Items, int Total)> ListarPorSucursalAsync(
        Guid sucursalGuid, int pagina, int pageSize, CancellationToken ct = default)
    {
        var q = _db.Valoraciones.AsNoTracking()
            .Where(x => x.SucursalGuid == sucursalGuid && !x.EsEliminado)
            .OrderByDescending(x => x.FechaPublicacionUtc);

        var total = await q.CountAsync(ct);
        var items = await q
            .Skip((pagina - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        return (items, total);
    }

    public async Task<IReadOnlyList<ValoracionEntity>> ListarPorClienteAsync(Guid clienteGuid, CancellationToken ct = default)
        => await _db.Valoraciones.AsNoTracking()
            .Where(x => x.ClienteGuid == clienteGuid && !x.EsEliminado)
            .OrderByDescending(x => x.FechaPublicacionUtc)
            .ToListAsync(ct);

    public async Task<ValoracionAgrupada?> ObtenerPromediosPorSucursalAsync(Guid sucursalGuid, CancellationToken ct = default)
    {
        var agg = await _db.Valoraciones.AsNoTracking()
            .Where(x => x.SucursalGuid == sucursalGuid && !x.EsEliminado)
            .GroupBy(_ => 1)
            .Select(g => new ValoracionAgrupada
            {
                PromedioGeneral = (double)g.Average(v => v.PuntuacionGeneral),
                PromedioLimpieza = (double)g.Average(v => v.PuntuacionLimpieza),
                PromedioConfort = (double)g.Average(v => v.PuntuacionConfort),
                PromedioUbicacion = (double)g.Average(v => v.PuntuacionUbicacion),
                PromedioInstalaciones = (double)g.Average(v => v.PuntuacionInstalaciones),
                PromedioPersonal = (double)g.Average(v => v.PuntuacionPersonal),
                PromedioCalidadPrecio = (double)g.Average(v => v.PuntuacionCalidadPrecio),
                Total = g.Count()
            })
            .FirstOrDefaultAsync(ct);

        return agg;
    }

    public async Task<int> ContarPorSucursalAsync(Guid sucursalGuid, CancellationToken ct = default)
        => await _db.Valoraciones.AsNoTracking()
            .CountAsync(x => x.SucursalGuid == sucursalGuid && !x.EsEliminado, ct);

    public async Task AgregarAsync(ValoracionEntity entity, CancellationToken ct = default)
        => await _db.Valoraciones.AddAsync(entity, ct);

    public async Task<(IReadOnlyList<ValoracionEntity> Items, int Total)> ListarPaginadoAsync(
        int pagina, int limite, CancellationToken ct = default)
    {
        var q = _db.Valoraciones.AsNoTracking()
            .Where(x => !x.EsEliminado)
            .OrderByDescending(x => x.FechaPublicacionUtc);
        var total = await q.CountAsync(ct);
        var items = await q
            .Skip((pagina - 1) * limite)
            .Take(limite)
            .ToListAsync(ct);
        return (items, total);
    }

    public async Task<ValoracionEntity?> ObtenerPorGuidAsync(Guid valoracionGuid, CancellationToken ct = default)
        => await _db.Valoraciones.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ValoracionGuid == valoracionGuid && !x.EsEliminado, ct);

    public async Task<ValoracionEntity?> ObtenerParaActualizarPorGuidAsync(Guid valoracionGuid, CancellationToken ct = default)
        => await _db.Valoraciones
            .FirstOrDefaultAsync(x => x.ValoracionGuid == valoracionGuid && !x.EsEliminado, ct);

    public void Actualizar(ValoracionEntity entity) => _db.Valoraciones.Update(entity);
}
