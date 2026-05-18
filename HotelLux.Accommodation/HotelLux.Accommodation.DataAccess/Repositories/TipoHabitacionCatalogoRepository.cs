using HotelLux.Accommodation.DataAccess.Context;
using HotelLux.Accommodation.DataAccess.Entities;
using HotelLux.Accommodation.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelLux.Accommodation.DataAccess.Repositories;

public class TipoHabitacionCatalogoRepository : ITipoHabitacionCatalogoRepository
{
    private readonly AccommodationDbContext _context;

    public TipoHabitacionCatalogoRepository(AccommodationDbContext context) => _context = context;

    public async Task<TipoHabitacionCatalogoEntity?> ObtenerAsync(int idTipo, int idCatalogo, CancellationToken ct = default)
        => await _context.TipoHabitacionCatalogos
            .FirstOrDefaultAsync(x => x.IdTipoHabitacion == idTipo && x.IdCatalogo == idCatalogo, ct);

    public async Task<IReadOnlyList<TipoHabitacionCatalogoEntity>> ListarPorTipoAsync(int idTipo, CancellationToken ct = default)
        => await _context.TipoHabitacionCatalogos.AsNoTracking()
            .Include(x => x.CatalogoServicio)
            .Where(x => x.IdTipoHabitacion == idTipo)
            .ToListAsync(ct);

    public async Task AgregarAsync(TipoHabitacionCatalogoEntity entity, CancellationToken ct = default)
        => await _context.TipoHabitacionCatalogos.AddAsync(entity, ct);

    public void Eliminar(TipoHabitacionCatalogoEntity entity) => _context.TipoHabitacionCatalogos.Remove(entity);
}
