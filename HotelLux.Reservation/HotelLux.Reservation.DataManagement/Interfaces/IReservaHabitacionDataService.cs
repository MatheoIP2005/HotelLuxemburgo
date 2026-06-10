using HotelLux.Reservation.DataManagement.Models;

namespace HotelLux.Reservation.DataManagement.Interfaces;

public interface IReservaHabitacionDataService
{
    Task<IReadOnlyList<ReservaHabitacionDataModel>> ListarPorReservaAsync(int idReserva, CancellationToken ct = default);
    Task<ReservaHabitacionDataModel?> ObtenerPorGuidAsync(Guid reservaHabitacionGuid, CancellationToken ct = default);
    Task ActualizarEstadoAsync(Guid reservaHabitacionGuid, string nuevoEstado, string usuario, CancellationToken ct = default);
    Task<ReservaHabitacionDataModel> InsertarLineaAsync(int idReserva, ReservaHabitacionDataModel line, CancellationToken ct = default);
    Task<bool> EliminarLineaAsync(int idReserva, Guid reservaHabitacionGuid, CancellationToken ct = default);

    Task<bool> ExisteSolapamientoConfirmadoAsync(
        Guid habitacionGuid,
        DateOnly fechaInicio,
        DateOnly fechaFin,
        Guid excludeReservaGuid,
        CancellationToken ct = default);
}
