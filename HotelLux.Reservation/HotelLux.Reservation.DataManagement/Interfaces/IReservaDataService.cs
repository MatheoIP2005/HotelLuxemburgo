using HotelLux.Reservation.DataManagement.Models;

namespace HotelLux.Reservation.DataManagement.Interfaces;

public interface IReservaDataService
{
    Task<ReservaDataModel?> ObtenerPorGuidAsync(Guid reservaGuid, CancellationToken ct = default);
    Task<ReservaDataModel?> ObtenerPorCodigoAsync(string codigoReserva, CancellationToken ct = default);
    Task<IReadOnlyList<ReservaDataModel>> ListarAsync(CancellationToken ct = default);
    Task<PagedDataResult<ReservaDataModel>> BuscarAsync(
        Guid? clienteGuid, Guid? sucursalGuid, string? estadoReserva,
        DateOnly? fechaDesde, DateOnly? fechaHasta, string? origenCanal,
        int pagina, int limite, CancellationToken ct = default);
    Task<ReservaDataModel> CrearAsync(ReservaDataModel model, CancellationToken ct = default);
    Task<ReservaDataModel?> ActualizarAsync(ReservaDataModel model, CancellationToken ct = default);
    Task<bool> EliminarLogicoAsync(Guid reservaGuid, string usuario, CancellationToken ct = default);
    Task RecalcularTotalesDesdeHabitacionesAsync(Guid reservaGuid, string usuario, CancellationToken ct = default);
}
