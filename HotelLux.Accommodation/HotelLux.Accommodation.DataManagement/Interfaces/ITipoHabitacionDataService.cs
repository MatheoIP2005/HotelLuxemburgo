using HotelLux.Accommodation.DataManagement.Models;

namespace HotelLux.Accommodation.DataManagement.Interfaces;

public interface ITipoHabitacionDataService
{
    Task<TipoHabitacionDataModel?> ObtenerPorIdAsync(int idTipoHabitacion, CancellationToken ct = default);
    Task<TipoHabitacionDataModel?> ObtenerPorGuidAsync(Guid guid, CancellationToken ct = default);
    Task<IReadOnlyList<TipoHabitacionDataModel>> ListarAsync(CancellationToken ct = default);
    Task<TipoHabitacionDataModel> CrearAsync(TipoHabitacionDataModel model, CancellationToken ct = default);
    Task<TipoHabitacionDataModel?> ActualizarAsync(TipoHabitacionDataModel model, CancellationToken ct = default);
    Task<bool> EliminarLogicoAsync(int idTipoHabitacion, string usuario, CancellationToken ct = default);
}
