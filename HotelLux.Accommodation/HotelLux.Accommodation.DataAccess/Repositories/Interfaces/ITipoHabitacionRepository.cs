using HotelLux.Accommodation.DataAccess.Entities;

namespace HotelLux.Accommodation.DataAccess.Repositories.Interfaces;

public interface ITipoHabitacionRepository
{
    Task<TipoHabitacionEntity?> ObtenerPorIdAsync(int idTipoHabitacion, CancellationToken ct = default);
    Task<TipoHabitacionEntity?> ObtenerPorGuidAsync(Guid guid, CancellationToken ct = default);
    Task<TipoHabitacionEntity?> ObtenerParaActualizarAsync(int idTipoHabitacion, CancellationToken ct = default);
    Task<IReadOnlyList<TipoHabitacionEntity>> ListarAsync(CancellationToken ct = default);
    Task AgregarAsync(TipoHabitacionEntity entity, CancellationToken ct = default);
    void Actualizar(TipoHabitacionEntity entity);
}
