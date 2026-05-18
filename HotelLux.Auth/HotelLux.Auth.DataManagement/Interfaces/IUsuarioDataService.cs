using HotelLux.Auth.DataManagement.Models;

namespace HotelLux.Auth.DataManagement.Interfaces;

public interface IUsuarioDataService
{
    Task<UsuarioDataModel?> ObtenerPorIdAsync(int idUsuario, CancellationToken cancellationToken = default);
    Task<UsuarioDataModel?> ObtenerPorGuidAsync(Guid usuarioGuid, CancellationToken cancellationToken = default);
    Task<UsuarioDataModel?> ObtenerPorUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UsuarioDataModel>> ListarAsync(CancellationToken cancellationToken = default);
    Task<LoginDataModel?> ObtenerParaLoginAsync(string username, CancellationToken cancellationToken = default);
    Task<UsuarioDataModel> CrearAsync(UsuarioDataModel model, CancellationToken cancellationToken = default);
    Task<UsuarioDataModel?> ActualizarAsync(UsuarioDataModel model, CancellationToken cancellationToken = default);
    Task<bool> EliminarLogicoAsync(int idUsuario, string usuario, CancellationToken cancellationToken = default);
}
