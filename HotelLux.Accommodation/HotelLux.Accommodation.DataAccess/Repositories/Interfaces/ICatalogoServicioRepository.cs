using HotelLux.Accommodation.DataAccess.Entities;

namespace HotelLux.Accommodation.DataAccess.Repositories.Interfaces;

public interface ICatalogoServicioRepository
{
    Task<CatalogoServicioEntity?> ObtenerPorIdAsync(int idCatalogo, CancellationToken ct = default);
    Task<CatalogoServicioEntity?> ObtenerPorGuidAsync(Guid guid, CancellationToken ct = default);
    Task<CatalogoServicioEntity?> ObtenerParaActualizarAsync(int idCatalogo, CancellationToken ct = default);
    Task<CatalogoServicioEntity?> ObtenerParaActualizarPorGuidAsync(Guid catalogoGuid, CancellationToken ct = default);
    Task<IReadOnlyList<CatalogoServicioEntity>> ListarAsync(CancellationToken ct = default);
    Task<IReadOnlyList<CatalogoServicioEntity>> ListarPorSucursalAsync(int? idSucursal, CancellationToken ct = default);
    Task AgregarAsync(CatalogoServicioEntity entity, CancellationToken ct = default);
    void Actualizar(CatalogoServicioEntity entity);
}
