using HotelLux.Auth.Business.DTOs.Roles;

namespace HotelLux.Auth.Business.Interfaces;

public interface IRolService
{
    Task<IReadOnlyList<RolDTO>> ListarAsync(CancellationToken cancellationToken = default);
    Task<RolDTO> ObtenerPorGuidAsync(Guid rolGuid, CancellationToken cancellationToken = default);
    Task<RolDTO> CrearAsync(RolCreateDTO dto, CancellationToken cancellationToken = default);
    Task<RolDTO> ActualizarAsync(Guid rolGuid, RolUpdateDTO dto, CancellationToken cancellationToken = default);
    Task InhabilitarAsync(Guid rolGuid, string usuario, CancellationToken cancellationToken = default);
    Task EliminarAsync(Guid rolGuid, string usuario, CancellationToken cancellationToken = default);
}
