using HotelLux.Auth.DataManagement.Models;

namespace HotelLux.Auth.DataManagement.Interfaces;

public interface IRolDataService
{
    Task<RolDataModel?> ObtenerPorIdAsync(int idRol, CancellationToken cancellationToken = default);
    Task<RolDataModel?> ObtenerPorGuidAsync(Guid rolGuid, CancellationToken cancellationToken = default);
    Task<RolDataModel?> ObtenerPorNombreAsync(string nombreRol, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RolDataModel>> ListarAsync(CancellationToken cancellationToken = default);
    Task<RolDataModel> CrearAsync(RolDataModel model, CancellationToken cancellationToken = default);
    Task<RolDataModel?> ActualizarAsync(RolDataModel model, CancellationToken cancellationToken = default);
    Task<bool> EliminarLogicoAsync(int idRol, string usuario, CancellationToken cancellationToken = default);
}
