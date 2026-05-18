using HotelLux.Accommodation.DataManagement.Models;

namespace HotelLux.Accommodation.DataManagement.Interfaces;

public interface ICatalogoServicioDataService
{
    Task<CatalogoServicioDataModel?> ObtenerPorIdAsync(int idCatalogo, CancellationToken ct = default);
    Task<CatalogoServicioDataModel?> ObtenerPorGuidAsync(Guid guid, CancellationToken ct = default);
    Task<IReadOnlyList<CatalogoServicioDataModel>> ListarAsync(CancellationToken ct = default);
    Task<IReadOnlyList<CatalogoServicioDataModel>> ListarPorSucursalAsync(int? idSucursal, CancellationToken ct = default);
    Task<CatalogoServicioDataModel> CrearAsync(CatalogoServicioDataModel model, CancellationToken ct = default);
    Task<CatalogoServicioDataModel?> ActualizarAsync(CatalogoServicioDataModel model, CancellationToken ct = default);
    Task DesactivarAsync(Guid catalogoGuid, string usuario, CancellationToken ct = default);
    Task<bool> EliminarLogicoAsync(int idCatalogo, string usuario, CancellationToken ct = default);
}
