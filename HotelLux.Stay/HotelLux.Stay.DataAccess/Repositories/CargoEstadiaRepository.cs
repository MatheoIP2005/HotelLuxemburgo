using HotelLux.Stay.DataAccess.Context;
using HotelLux.Stay.DataAccess.Entities;
using HotelLux.Stay.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelLux.Stay.DataAccess.Repositories;

public class CargoEstadiaRepository : ICargoEstadiaRepository
{
    private readonly StayDbContext _db;
    public CargoEstadiaRepository(StayDbContext db) => _db = db;

    public async Task<IReadOnlyList<CargoEstadiaEntity>> ListarPorEstadiaAsync(int idEstadia, CancellationToken ct = default)
        => await _db.Cargos.AsNoTracking()
            .Where(x => x.IdEstadia == idEstadia)
            .OrderByDescending(x => x.FechaConsumoUtc)
            .ToListAsync(ct);

    public async Task<CargoEstadiaEntity?> ObtenerPorGuidAsync(Guid cargoGuid, CancellationToken ct = default)
        => await _db.Cargos.AsNoTracking()
            .FirstOrDefaultAsync(x => x.CargoGuid == cargoGuid, ct);

    public async Task<CargoEstadiaEntity?> ObtenerParaActualizarPorGuidAsync(Guid cargoGuid, CancellationToken ct = default)
        => await _db.Cargos
            .FirstOrDefaultAsync(x => x.CargoGuid == cargoGuid, ct);

    public void Actualizar(CargoEstadiaEntity entity) => _db.Cargos.Update(entity);

    public async Task AgregarAsync(CargoEstadiaEntity entity, CancellationToken ct = default)
        => await _db.Cargos.AddAsync(entity, ct);
}
