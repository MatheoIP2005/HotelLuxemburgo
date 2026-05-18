using HotelLux.Accommodation.DataManagement.Models;

namespace HotelLux.Accommodation.DataManagement.Interfaces;

public interface ISucursalImagenDataService
{
    Task<IReadOnlyList<SucursalImagenDataModel>> ListarPorSucursalAsync(int idSucursal, CancellationToken ct = default);
    Task<IReadOnlyList<SucursalImagenDataModel>> ListarPorSucursalGuidAsync(Guid sucursalGuid, CancellationToken ct = default);
    Task<SucursalImagenDataModel> CrearAsync(SucursalImagenDataModel model, CancellationToken ct = default);
    Task EliminarAsync(Guid imagenGuid, CancellationToken ct = default);
}
