using HotelLux.Accommodation.DataAccess.Context;
using HotelLux.Accommodation.DataAccess.Entities;
using HotelLux.Accommodation.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelLux.Accommodation.DataAccess.Repositories;

public class SucursalImagenRepository : ISucursalImagenRepository
{
    private readonly AccommodationDbContext _context;

    public SucursalImagenRepository(AccommodationDbContext context) => _context = context;

    public async Task<IReadOnlyList<SucursalImagenEntity>> ListarPorSucursalAsync(int idSucursal, CancellationToken ct = default)
        => await _context.SucursalImagenes.AsNoTracking()
            .Where(x => x.IdSucursal == idSucursal)
            .OrderBy(x => x.OrdenVisualizacion)
            .ToListAsync(ct);

    public async Task<SucursalImagenEntity?> ObtenerPorGuidAsync(Guid imagenGuid, CancellationToken ct = default)
        => await _context.SucursalImagenes.FirstOrDefaultAsync(x => x.SucursalImagenGuid == imagenGuid, ct);

    public async Task AgregarAsync(SucursalImagenEntity entity, CancellationToken ct = default)
        => await _context.SucursalImagenes.AddAsync(entity, ct);

    public void Eliminar(SucursalImagenEntity entity) => _context.SucursalImagenes.Remove(entity);
}
