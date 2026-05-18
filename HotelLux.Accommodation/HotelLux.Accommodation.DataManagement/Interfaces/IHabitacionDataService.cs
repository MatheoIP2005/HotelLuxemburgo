using HotelLux.Accommodation.DataManagement.Models;

namespace HotelLux.Accommodation.DataManagement.Interfaces;

public interface IHabitacionDataService
{
    Task<HabitacionDataModel?> ObtenerPorIdAsync(int idHabitacion, CancellationToken ct = default);
    Task<HabitacionDataModel?> ObtenerPorGuidAsync(Guid guid, CancellationToken ct = default);
    Task<IReadOnlyList<HabitacionDataModel>> ListarAsync(CancellationToken ct = default);
    Task<IReadOnlyList<HabitacionDataModel>> ListarPorSucursalAsync(Guid sucursalGuid, CancellationToken ct = default);
    Task<IReadOnlyList<HabitacionDataModel>> ListarDisponiblesAsync(Guid sucursalGuid, DateOnly inicio, DateOnly fin, CancellationToken ct = default);
    Task<HabitacionDataModel> CrearAsync(HabitacionDataModel model, CancellationToken ct = default);
    Task<HabitacionDataModel?> ActualizarAsync(HabitacionDataModel model, CancellationToken ct = default);
    Task CambiarEstadoAsync(Guid habitacionGuid, string nuevoEstado, string usuario, CancellationToken ct = default);
    Task<bool> EliminarLogicoAsync(int idHabitacion, string usuario, CancellationToken ct = default);
}
