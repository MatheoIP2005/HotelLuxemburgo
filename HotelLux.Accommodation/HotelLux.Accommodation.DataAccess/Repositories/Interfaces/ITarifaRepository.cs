using HotelLux.Accommodation.DataAccess.Entities;

namespace HotelLux.Accommodation.DataAccess.Repositories.Interfaces;

public interface ITarifaRepository
{
    Task<TarifaEntity?> ObtenerPorIdAsync(int idTarifa, CancellationToken ct = default);
    Task<TarifaEntity?> ObtenerPorGuidAsync(Guid guid, CancellationToken ct = default);
    Task<TarifaEntity?> ObtenerParaActualizarAsync(int idTarifa, CancellationToken ct = default);
    Task<TarifaEntity?> ObtenerParaActualizarPorGuidAsync(Guid tarifaGuid, CancellationToken ct = default);
    Task<IReadOnlyList<TarifaEntity>> ListarAsync(CancellationToken ct = default);
    Task<IReadOnlyList<TarifaEntity>> ListarPorSucursalAsync(int idSucursal, CancellationToken ct = default);
    Task AgregarAsync(TarifaEntity entity, CancellationToken ct = default);
    void Actualizar(TarifaEntity entity);
}
