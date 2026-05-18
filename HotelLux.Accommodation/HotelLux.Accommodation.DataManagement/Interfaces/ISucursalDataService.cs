using HotelLux.Accommodation.DataManagement.Models;

namespace HotelLux.Accommodation.DataManagement.Interfaces;

public interface ISucursalDataService
{
    Task<SucursalDataModel?> ObtenerPorIdAsync(int idSucursal, CancellationToken ct = default);
    Task<SucursalDataModel?> ObtenerPorGuidAsync(Guid guid, CancellationToken ct = default);
    Task<IReadOnlyList<SucursalDataModel>> ListarAsync(CancellationToken ct = default);
    Task<SucursalDataModel> CrearAsync(SucursalDataModel model, CancellationToken ct = default);
    Task<SucursalDataModel?> ActualizarAsync(SucursalDataModel model, CancellationToken ct = default);
    Task<bool> EliminarLogicoAsync(int idSucursal, string usuario, CancellationToken ct = default);
}
