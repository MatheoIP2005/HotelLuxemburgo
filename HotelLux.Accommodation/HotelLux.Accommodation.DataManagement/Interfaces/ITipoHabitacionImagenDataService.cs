using HotelLux.Accommodation.DataManagement.Models;

namespace HotelLux.Accommodation.DataManagement.Interfaces;

public interface ITipoHabitacionImagenDataService
{
    Task<IReadOnlyList<TipoHabitacionImagenDataModel>> ListarPorTipoAsync(Guid tipoGuid, CancellationToken ct = default);
    Task<TipoHabitacionImagenDataModel> CrearAsync(TipoHabitacionImagenDataModel model, CancellationToken ct = default);
    Task EliminarAsync(Guid tipoGuid, int idImagen, CancellationToken ct = default);
}
