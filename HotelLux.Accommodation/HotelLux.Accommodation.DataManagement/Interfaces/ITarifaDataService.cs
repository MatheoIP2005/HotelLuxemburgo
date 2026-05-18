using HotelLux.Accommodation.DataManagement.Models;

namespace HotelLux.Accommodation.DataManagement.Interfaces;

public interface ITarifaDataService
{
    Task<TarifaDataModel?> ObtenerPorIdAsync(int idTarifa, CancellationToken ct = default);
    Task<TarifaDataModel?> ObtenerPorGuidAsync(Guid guid, CancellationToken ct = default);
    Task<IReadOnlyList<TarifaDataModel>> ListarAsync(CancellationToken ct = default);
    Task<IReadOnlyList<TarifaDataModel>> ListarPorSucursalAsync(Guid sucursalGuid, CancellationToken ct = default);
    Task<TarifaDataModel> CrearAsync(TarifaDataModel model, CancellationToken ct = default);
    Task<TarifaDataModel?> ActualizarAsync(TarifaDataModel model, CancellationToken ct = default);
    Task DesactivarAsync(Guid tarifaGuid, string usuario, CancellationToken ct = default);
    Task<bool> EliminarLogicoAsync(int idTarifa, string usuario, CancellationToken ct = default);
}
