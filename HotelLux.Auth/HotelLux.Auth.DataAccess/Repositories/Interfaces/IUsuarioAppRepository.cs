using HotelLux.Auth.DataAccess.Entities;

namespace HotelLux.Auth.DataAccess.Repositories.Interfaces;

public interface IUsuarioAppRepository
{
    Task<UsuarioAppEntity?> ObtenerPorIdAsync(int idUsuario, CancellationToken cancellationToken = default);
    Task<UsuarioAppEntity?> ObtenerPorGuidAsync(Guid usuarioGuid, CancellationToken cancellationToken = default);
    Task<UsuarioAppEntity?> ObtenerParaActualizarAsync(int idUsuario, CancellationToken cancellationToken = default);
    Task<UsuarioAppEntity?> ObtenerPorUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<UsuarioAppEntity?> ObtenerPorCorreoAsync(string correo, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UsuarioAppEntity>> ListarAsync(CancellationToken cancellationToken = default);
    Task AgregarAsync(UsuarioAppEntity usuario, CancellationToken cancellationToken = default);
    void Actualizar(UsuarioAppEntity usuario);
    void EliminarLogico(UsuarioAppEntity usuario);

    Task<UsuarioRolEntity?> ObtenerUsuarioRolPorUsuarioYRolAsync(int idUsuario, int idRol, CancellationToken cancellationToken = default);
    Task AgregarUsuarioRolAsync(UsuarioRolEntity usuarioRol, CancellationToken cancellationToken = default);
    void ActualizarUsuarioRol(UsuarioRolEntity usuarioRol);
}
