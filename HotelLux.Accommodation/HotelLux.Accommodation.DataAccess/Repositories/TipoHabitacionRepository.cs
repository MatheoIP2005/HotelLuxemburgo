using HotelLux.Accommodation.DataAccess.Context;
using HotelLux.Accommodation.DataAccess.Entities;
using HotelLux.Accommodation.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelLux.Accommodation.DataAccess.Repositories;

public class TipoHabitacionRepository : ITipoHabitacionRepository
{
    private readonly AccommodationDbContext _context;

    public TipoHabitacionRepository(AccommodationDbContext context) => _context = context;

    public async Task<TipoHabitacionEntity?> ObtenerPorIdAsync(int idTipoHabitacion, CancellationToken ct = default)
        => await _context.TiposHabitacion.AsNoTracking()
            .FirstOrDefaultAsync(x => x.IdTipoHabitacion == idTipoHabitacion && !x.EsEliminado, ct);

    public async Task<TipoHabitacionEntity?> ObtenerPorGuidAsync(Guid guid, CancellationToken ct = default)
        => await _context.TiposHabitacion.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TipoHabitacionGuid == guid && !x.EsEliminado, ct);

    public async Task<TipoHabitacionEntity?> ObtenerParaActualizarAsync(int idTipoHabitacion, CancellationToken ct = default)
        => await _context.TiposHabitacion.FirstOrDefaultAsync(x => x.IdTipoHabitacion == idTipoHabitacion && !x.EsEliminado, ct);

    public async Task<IReadOnlyList<TipoHabitacionEntity>> ListarAsync(CancellationToken ct = default)
        => await _context.TiposHabitacion.AsNoTracking()
            .Where(x => !x.EsEliminado)
            .OrderBy(x => x.NombreTipoHabitacion)
            .ToListAsync(ct);

    public async Task AgregarAsync(TipoHabitacionEntity entity, CancellationToken ct = default)
        => await _context.TiposHabitacion.AddAsync(entity, ct);

    public void Actualizar(TipoHabitacionEntity entity) => _context.TiposHabitacion.Update(entity);
}
