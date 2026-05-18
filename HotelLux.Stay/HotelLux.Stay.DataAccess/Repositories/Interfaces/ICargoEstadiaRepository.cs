using HotelLux.Stay.DataAccess.Entities;

namespace HotelLux.Stay.DataAccess.Repositories.Interfaces;

public interface ICargoEstadiaRepository
{
    Task<IReadOnlyList<CargoEstadiaEntity>> ListarPorEstadiaAsync(int idEstadia, CancellationToken ct = default);
    Task<CargoEstadiaEntity?> ObtenerPorGuidAsync(Guid cargoGuid, CancellationToken ct = default);
    Task<CargoEstadiaEntity?> ObtenerParaActualizarPorGuidAsync(Guid cargoGuid, CancellationToken ct = default);
    Task AgregarAsync(CargoEstadiaEntity entity, CancellationToken ct = default);
    void Actualizar(CargoEstadiaEntity entity);
}
