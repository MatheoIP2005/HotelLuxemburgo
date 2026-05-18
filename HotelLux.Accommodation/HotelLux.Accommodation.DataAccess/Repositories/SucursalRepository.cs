using HotelLux.Accommodation.DataAccess.Context;
using HotelLux.Accommodation.DataAccess.Entities;
using HotelLux.Accommodation.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelLux.Accommodation.DataAccess.Repositories;

public class SucursalRepository : ISucursalRepository
{
    private readonly AccommodationDbContext _context;

    public SucursalRepository(AccommodationDbContext context) => _context = context;

    public async Task<SucursalEntity?> ObtenerPorIdAsync(int idSucursal, CancellationToken ct = default)
        => await _context.Sucursales.AsNoTracking()
            .FirstOrDefaultAsync(x => x.IdSucursal == idSucursal && !x.EsEliminado, ct);

    public async Task<SucursalEntity?> ObtenerPorGuidAsync(Guid guid, CancellationToken ct = default)
        => await _context.Sucursales.AsNoTracking()
            .FirstOrDefaultAsync(x => x.SucursalGuid == guid && !x.EsEliminado, ct);

    public async Task<SucursalEntity?> ObtenerParaActualizarAsync(int idSucursal, CancellationToken ct = default)
        => await _context.Sucursales.FirstOrDefaultAsync(x => x.IdSucursal == idSucursal && !x.EsEliminado, ct);

    public async Task<IReadOnlyList<SucursalEntity>> ListarAsync(CancellationToken ct = default)
        => await _context.Sucursales.AsNoTracking()
            .Where(x => !x.EsEliminado)
            .OrderBy(x => x.NombreSucursal)
            .ToListAsync(ct);

    public async Task AgregarAsync(SucursalEntity entity, CancellationToken ct = default)
        => await _context.Sucursales.AddAsync(entity, ct);

    public void Actualizar(SucursalEntity entity) => _context.Sucursales.Update(entity);
}
