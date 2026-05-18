using HotelLux.Accommodation.DataAccess.Context;
using HotelLux.Accommodation.DataAccess.Entities;
using HotelLux.Accommodation.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelLux.Accommodation.DataAccess.Repositories;

public class CatalogoServicioRepository : ICatalogoServicioRepository
{
    private readonly AccommodationDbContext _context;

    public CatalogoServicioRepository(AccommodationDbContext context) => _context = context;

    public async Task<CatalogoServicioEntity?> ObtenerPorIdAsync(int idCatalogo, CancellationToken ct = default)
        => await _context.CatalogoServicios.AsNoTracking()
            .Include(c => c.Sucursal)
            .FirstOrDefaultAsync(x => x.IdCatalogo == idCatalogo && !x.EsEliminado, ct);

    public async Task<CatalogoServicioEntity?> ObtenerPorGuidAsync(Guid guid, CancellationToken ct = default)
        => await _context.CatalogoServicios.AsNoTracking()
            .Include(c => c.Sucursal)
            .FirstOrDefaultAsync(x => x.CatalogoGuid == guid && !x.EsEliminado, ct);

    public async Task<CatalogoServicioEntity?> ObtenerParaActualizarAsync(int idCatalogo, CancellationToken ct = default)
        => await _context.CatalogoServicios.FirstOrDefaultAsync(x => x.IdCatalogo == idCatalogo && !x.EsEliminado, ct);

    public async Task<CatalogoServicioEntity?> ObtenerParaActualizarPorGuidAsync(Guid catalogoGuid, CancellationToken ct = default)
        => await _context.CatalogoServicios.FirstOrDefaultAsync(x => x.CatalogoGuid == catalogoGuid && !x.EsEliminado, ct);

    public async Task<IReadOnlyList<CatalogoServicioEntity>> ListarAsync(CancellationToken ct = default)
        => await _context.CatalogoServicios.AsNoTracking()
            .Include(c => c.Sucursal)
            .Where(x => !x.EsEliminado)
            .OrderBy(x => x.NombreCatalogo)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<CatalogoServicioEntity>> ListarPorSucursalAsync(int? idSucursal, CancellationToken ct = default)
    {
        IQueryable<CatalogoServicioEntity> q = _context.CatalogoServicios.AsNoTracking()
            .Include(c => c.Sucursal)
            .Where(x => !x.EsEliminado);
        q = idSucursal.HasValue
            ? q.Where(x => x.IdSucursal == idSucursal)
            : q.Where(x => x.IdSucursal == null);
        return await q.OrderBy(x => x.NombreCatalogo).ToListAsync(ct);
    }

    public async Task AgregarAsync(CatalogoServicioEntity entity, CancellationToken ct = default)
        => await _context.CatalogoServicios.AddAsync(entity, ct);

    public void Actualizar(CatalogoServicioEntity entity) => _context.CatalogoServicios.Update(entity);
}
