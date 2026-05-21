using HotelLux.Reservation.Business.DTOs.Common;
using HotelLux.Reservation.Business.DTOs.Reserva;
using HotelLux.Reservation.Business.DTOs.ReservaHabitacion;

namespace HotelLux.Reservation.Business.Interfaces;

public interface IReservaService
{
    Task<ReservaDTO> ObtenerPorGuidAsync(Guid reservaGuid, CancellationToken ct = default);
    Task<IReadOnlyList<ReservaDTO>> ListarAsync(CancellationToken ct = default);
    Task<PagedResultDTO<ReservaDTO>> BuscarAsync(ReservaFiltroDTO filtro, CancellationToken ct = default);
    Task<IReadOnlyList<ReservaHabitacionDTO>> ListarHabitacionesAsync(Guid reservaGuid, CancellationToken ct = default);
    Task<ReservaDTO> CrearAsync(ReservaCreateDTO dto, CancellationToken ct = default);
    Task<ReservaDTO> ConfirmarAsync(Guid reservaGuid, string usuario, CancellationToken ct = default);
    Task<ReservaDTO> CancelarAsync(Guid reservaGuid, string motivo, string usuario, CancellationToken ct = default);
    Task EliminarAsync(Guid reservaGuid, string usuario, CancellationToken ct = default);
    Task<ReservaHabitacionDTO> AgregarHabitacionAsync(Guid reservaGuid, ReservaHabitacionCreateDTO dto, string usuario, CancellationToken ct = default);
    Task EliminarHabitacionAsync(Guid reservaGuid, Guid reservaHabitacionGuid, string usuario, CancellationToken ct = default);
    Task EliminarHabitacionPorIdAsync(Guid reservaGuid, int idReservaHabitacion, string usuario, CancellationToken ct = default);
}
