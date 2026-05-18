using HotelLux.Accommodation.DataAccess.Entities;

namespace HotelLux.Accommodation.DataAccess.Repositories.Interfaces;

public interface IHabitacionRepository
{
    Task<HabitacionEntity?> ObtenerPorIdAsync(int idHabitacion, CancellationToken ct = default);
    Task<HabitacionEntity?> ObtenerPorGuidAsync(Guid guid, CancellationToken ct = default);
    Task<HabitacionEntity?> ObtenerParaActualizarAsync(int idHabitacion, CancellationToken ct = default);
    Task<HabitacionEntity?> ObtenerParaActualizarPorGuidAsync(Guid habitacionGuid, CancellationToken ct = default);
    Task<IReadOnlyList<HabitacionEntity>> ListarAsync(CancellationToken ct = default);
    Task<IReadOnlyList<HabitacionEntity>> ListarPorSucursalAsync(int idSucursal, CancellationToken ct = default);
    Task<IReadOnlyList<HabitacionEntity>> ListarDisponiblesAsync(int idSucursal, DateOnly inicio, DateOnly fin, CancellationToken ct = default);
    Task AgregarAsync(HabitacionEntity entity, CancellationToken ct = default);
    void Actualizar(HabitacionEntity entity);
}
