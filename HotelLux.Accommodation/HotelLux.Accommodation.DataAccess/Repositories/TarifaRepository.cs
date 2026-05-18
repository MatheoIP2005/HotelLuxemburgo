using HotelLux.Accommodation.DataAccess.Context;
using HotelLux.Accommodation.DataAccess.Entities;
using HotelLux.Accommodation.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelLux.Accommodation.DataAccess.Repositories;

public class TarifaRepository : ITarifaRepository
{
    private readonly AccommodationDbContext _context;

    public TarifaRepository(AccommodationDbContext context) => _context = context;

    public async Task<TarifaEntity?> ObtenerPorIdAsync(int idTarifa, CancellationToken ct = default)
        => await _context.Tarifas.AsNoTracking()
            .Include(t => t.Sucursal)
            .Include(t => t.TipoHabitacion)
            .FirstOrDefaultAsync(x => x.IdTarifa == idTarifa && !x.EsEliminado, ct);

    public async Task<TarifaEntity?> ObtenerPorGuidAsync(Guid guid, CancellationToken ct = default)
        => await _context.Tarifas.AsNoTracking()
            .Include(t => t.Sucursal)
            .Include(t => t.TipoHabitacion)
            .FirstOrDefaultAsync(x => x.TarifaGuid == guid && !x.EsEliminado, ct);

    public async Task<TarifaEntity?> ObtenerParaActualizarAsync(int idTarifa, CancellationToken ct = default)
        => await _context.Tarifas.FirstOrDefaultAsync(x => x.IdTarifa == idTarifa && !x.EsEliminado, ct);

    public async Task<TarifaEntity?> ObtenerParaActualizarPorGuidAsync(Guid tarifaGuid, CancellationToken ct = default)
        => await _context.Tarifas.FirstOrDefaultAsync(x => x.TarifaGuid == tarifaGuid && !x.EsEliminado, ct);

    public async Task<IReadOnlyList<TarifaEntity>> ListarAsync(CancellationToken ct = default)
        => await _context.Tarifas.AsNoTracking()
            .Include(t => t.Sucursal)
            .Include(t => t.TipoHabitacion)
            .Where(x => !x.EsEliminado)
            .OrderBy(x => x.CodigoTarifa)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<TarifaEntity>> ListarPorSucursalAsync(int idSucursal, CancellationToken ct = default)
        => await _context.Tarifas.AsNoTracking()
            .Include(t => t.Sucursal)
            .Include(t => t.TipoHabitacion)
            .Where(x => x.IdSucursal == idSucursal && !x.EsEliminado)
            .OrderBy(x => x.Prioridad)
            .ToListAsync(ct);

    public async Task AgregarAsync(TarifaEntity entity, CancellationToken ct = default)
        => await _context.Tarifas.AddAsync(entity, ct);

    public void Actualizar(TarifaEntity entity) => _context.Tarifas.Update(entity);
}
