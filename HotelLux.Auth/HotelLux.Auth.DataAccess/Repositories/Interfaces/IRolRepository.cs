using HotelLux.Auth.DataAccess.Entities;

namespace HotelLux.Auth.DataAccess.Repositories.Interfaces;

public interface IRolRepository
{
    Task<RolEntity?> ObtenerPorIdAsync(int idRol, CancellationToken cancellationToken = default);
    Task<RolEntity?> ObtenerPorGuidAsync(Guid rolGuid, CancellationToken cancellationToken = default);
    Task<RolEntity?> ObtenerParaActualizarAsync(int idRol, CancellationToken cancellationToken = default);
    Task<RolEntity?> ObtenerPorNombreAsync(string nombreRol, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RolEntity>> ListarAsync(CancellationToken cancellationToken = default);
    Task AgregarAsync(RolEntity rol, CancellationToken cancellationToken = default);
    void Actualizar(RolEntity rol);
    void EliminarLogico(RolEntity rol);
}
