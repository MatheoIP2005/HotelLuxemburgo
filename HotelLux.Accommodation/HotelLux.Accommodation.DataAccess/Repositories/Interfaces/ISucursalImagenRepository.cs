using HotelLux.Accommodation.DataAccess.Entities;

namespace HotelLux.Accommodation.DataAccess.Repositories.Interfaces;

public interface ISucursalImagenRepository
{
    Task<IReadOnlyList<SucursalImagenEntity>> ListarPorSucursalAsync(int idSucursal, CancellationToken ct = default);
    Task<SucursalImagenEntity?> ObtenerPorGuidAsync(Guid imagenGuid, CancellationToken ct = default);
    Task AgregarAsync(SucursalImagenEntity entity, CancellationToken ct = default);
    void Eliminar(SucursalImagenEntity entity);
}
