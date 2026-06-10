using HotelLux.Reservation.DataAccess.Entities;

namespace HotelLux.Reservation.DataAccess.Repositories.Interfaces;

public interface IReservaHabitacionRepository
{
    Task<ReservaHabitacionEntity?> ObtenerPorGuidAsync(Guid reservaHabitacionGuid, CancellationToken ct = default);
    Task<ReservaHabitacionEntity?> ObtenerParaActualizarPorGuidAsync(Guid reservaHabitacionGuid, CancellationToken ct = default);
    Task<IReadOnlyList<ReservaHabitacionEntity>> ListarPorReservaAsync(int idReserva, CancellationToken ct = default);
    Task AgregarRangoAsync(IEnumerable<ReservaHabitacionEntity> entities, CancellationToken ct = default);
    Task AgregarAsync(ReservaHabitacionEntity entity, CancellationToken ct = default);
    void Actualizar(ReservaHabitacionEntity entity);
    void Eliminar(ReservaHabitacionEntity entity);

    Task<bool> ExisteSolapamientoConfirmadoAsync(
        Guid habitacionGuid,
        DateOnly fechaInicio,
        DateOnly fechaFin,
        Guid excludeReservaGuid,
        CancellationToken ct = default);
}
