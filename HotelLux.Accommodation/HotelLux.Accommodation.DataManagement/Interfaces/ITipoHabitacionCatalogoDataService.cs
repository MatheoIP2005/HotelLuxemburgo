using HotelLux.Accommodation.DataManagement.Models;

namespace HotelLux.Accommodation.DataManagement.Interfaces;

public interface ITipoHabitacionCatalogoDataService
{
    Task<IReadOnlyList<CatalogoServicioDataModel>> ListarPorTipoAsync(int idTipoHabitacion, CancellationToken ct = default);
    Task<TipoHabitacionCatalogoDataModel?> ObtenerAsync(int idTipo, int idCatalogo, CancellationToken ct = default);
    Task AsignarAsync(int idTipoHabitacion, int idCatalogo, string usuario, CancellationToken ct = default);
    Task RemoverAsync(int idTipoHabitacion, int idCatalogo, CancellationToken ct = default);
    Task RemoverPorIdAsync(int idTipoHabitacion, int idTipoHabCatalogo, CancellationToken ct = default);
}
