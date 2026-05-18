using HotelLux.Accommodation.DataAccess.Context;
using HotelLux.Accommodation.DataAccess.Entities;
using HotelLux.Accommodation.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelLux.Accommodation.DataAccess.Repositories;

public class HabitacionRepository : IHabitacionRepository
{
    private readonly AccommodationDbContext _context;

    public HabitacionRepository(AccommodationDbContext context) => _context = context;

    public async Task<HabitacionEntity?> ObtenerPorIdAsync(int idHabitacion, CancellationToken ct = default)
        => await _context.Habitaciones.AsNoTracking()
            .Include(h => h.Sucursal)
            .Include(h => h.TipoHabitacion)
            .FirstOrDefaultAsync(x => x.IdHabitacion == idHabitacion && !x.EsEliminado, ct);

    public async Task<HabitacionEntity?> ObtenerPorGuidAsync(Guid guid, CancellationToken ct = default)
        => await _context.Habitaciones.AsNoTracking()
            .Include(h => h.Sucursal)
            .Include(h => h.TipoHabitacion)
            .FirstOrDefaultAsync(x => x.HabitacionGuid == guid && !x.EsEliminado, ct);

    public async Task<HabitacionEntity?> ObtenerParaActualizarAsync(int idHabitacion, CancellationToken ct = default)
        => await _context.Habitaciones.FirstOrDefaultAsync(x => x.IdHabitacion == idHabitacion && !x.EsEliminado, ct);

    public async Task<HabitacionEntity?> ObtenerParaActualizarPorGuidAsync(Guid habitacionGuid, CancellationToken ct = default)
        => await _context.Habitaciones.FirstOrDefaultAsync(x => x.HabitacionGuid == habitacionGuid && !x.EsEliminado, ct);

    public async Task<IReadOnlyList<HabitacionEntity>> ListarAsync(CancellationToken ct = default)
        => await _context.Habitaciones.AsNoTracking()
            .Include(h => h.Sucursal)
            .Include(h => h.TipoHabitacion)
            .Where(x => !x.EsEliminado)
            .OrderBy(x => x.NumeroHabitacion)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<HabitacionEntity>> ListarPorSucursalAsync(int idSucursal, CancellationToken ct = default)
        => await _context.Habitaciones.AsNoTracking()
            .Include(h => h.Sucursal)
            .Include(h => h.TipoHabitacion)
            .Where(x => x.IdSucursal == idSucursal && !x.EsEliminado)
            .OrderBy(x => x.NumeroHabitacion)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<HabitacionEntity>> ListarDisponiblesAsync(int idSucursal, DateOnly inicio, DateOnly fin, CancellationToken ct = default)
        => await _context.Habitaciones.AsNoTracking()
            .Include(h => h.Sucursal)
            .Include(h => h.TipoHabitacion)
            .Where(x => x.IdSucursal == idSucursal && !x.EsEliminado && x.EstadoHabitacion == "DIS")
            .OrderBy(x => x.NumeroHabitacion)
            .ToListAsync(ct);

    public async Task AgregarAsync(HabitacionEntity entity, CancellationToken ct = default)
        => await _context.Habitaciones.AddAsync(entity, ct);

    public void Actualizar(HabitacionEntity entity) => _context.Habitaciones.Update(entity);
}
