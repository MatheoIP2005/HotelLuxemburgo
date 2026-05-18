using HotelLux.Stay.DataManagement.Models;

namespace HotelLux.Stay.DataManagement.Interfaces;

public interface IEstadiaDataService
{
    Task<EstadiaDataModel?> ObtenerPorGuidAsync(Guid estadiaGuid, CancellationToken ct = default);
    Task<EstadiaDataModel?> ObtenerActivaPorReservaGuidAsync(Guid reservaGuid, CancellationToken ct = default);
    Task<EstadiaDataModel?> ObtenerActivaPorReservaHabitacionGuidAsync(Guid reservaHabitacionGuid, CancellationToken ct = default);
    Task<EstadiaDataModel> CrearAsync(EstadiaDataModel model, CancellationToken ct = default);
    Task<EstadiaDataModel?> ActualizarAsync(EstadiaDataModel model, CancellationToken ct = default);
    Task<(IReadOnlyList<EstadiaDataModel> Items, int Total)> ListarAsync(
        string? estado, Guid? sucursalGuid, int pagina, int limite, CancellationToken ct = default);
}
