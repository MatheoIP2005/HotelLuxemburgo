using HotelLux.Accommodation.DataAccess.Entities;

namespace HotelLux.Accommodation.DataAccess.Repositories.Interfaces;

public interface ISucursalRepository
{
    Task<SucursalEntity?> ObtenerPorIdAsync(int idSucursal, CancellationToken ct = default);
    Task<SucursalEntity?> ObtenerPorGuidAsync(Guid guid, CancellationToken ct = default);
    Task<SucursalEntity?> ObtenerParaActualizarAsync(int idSucursal, CancellationToken ct = default);
    Task<IReadOnlyList<SucursalEntity>> ListarAsync(CancellationToken ct = default);
    Task AgregarAsync(SucursalEntity entity, CancellationToken ct = default);
    void Actualizar(SucursalEntity entity);
}
