using HotelLux.Accommodation.DataAccess.Context;
using HotelLux.Accommodation.DataAccess.Entities;
using HotelLux.Accommodation.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelLux.Accommodation.DataAccess.Repositories;

public class TipoHabitacionImagenRepository : ITipoHabitacionImagenRepository
{
    private readonly AccommodationDbContext _context;

    public TipoHabitacionImagenRepository(AccommodationDbContext context) => _context = context;

    public async Task<IReadOnlyList<TipoHabitacionImagenEntity>> ListarPorTipoAsync(int idTipoHabitacion, CancellationToken ct = default)
        => await _context.TipoHabitacionImagenes.AsNoTracking()
            .Where(x => x.IdTipoHabitacion == idTipoHabitacion)
            .OrderBy(x => x.OrdenVisualizacion)
            .ToListAsync(ct);

    public async Task AgregarAsync(TipoHabitacionImagenEntity entity, CancellationToken ct = default)
        => await _context.TipoHabitacionImagenes.AddAsync(entity, ct);

    public async Task<TipoHabitacionImagenEntity?> ObtenerPorIdAsync(int idImagen, CancellationToken ct = default)
        => await _context.TipoHabitacionImagenes.FirstOrDefaultAsync(x => x.IdTipoHabitacionImagen == idImagen, ct);

    public void Eliminar(TipoHabitacionImagenEntity entity) => _context.TipoHabitacionImagenes.Remove(entity);
}
