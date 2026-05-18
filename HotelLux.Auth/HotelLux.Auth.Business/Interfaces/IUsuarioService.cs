using HotelLux.Auth.Business.DTOs.Roles;
using HotelLux.Auth.Business.DTOs.Usuarios;

namespace HotelLux.Auth.Business.Interfaces;

public interface IUsuarioService
{
    Task<IReadOnlyList<UsuarioDTO>> ListarAsync(CancellationToken cancellationToken = default);
    Task<UsuarioDTO> ObtenerPorGuidAsync(Guid usuarioGuid, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RolDTO>> ListarRolesAsync(Guid usuarioGuid, CancellationToken cancellationToken = default);
    Task<UsuarioDTO> CrearAsync(UsuarioCreateDTO dto, CancellationToken cancellationToken = default);
    Task<UsuarioDTO> ActualizarAsync(Guid usuarioGuid, UsuarioUpdateDTO dto, CancellationToken cancellationToken = default);
    Task InhabilitarAsync(Guid usuarioGuid, string motivo, string usuario, CancellationToken cancellationToken = default);
    Task EliminarAsync(Guid usuarioGuid, string usuario, CancellationToken cancellationToken = default);
    Task AsignarRolAsync(Guid usuarioGuid, Guid rolGuid, string usuario, CancellationToken cancellationToken = default);
    Task RemoverRolAsync(Guid usuarioGuid, Guid rolGuid, string usuario, CancellationToken cancellationToken = default);
}
