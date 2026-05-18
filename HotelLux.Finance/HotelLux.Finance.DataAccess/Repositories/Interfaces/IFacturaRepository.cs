using HotelLux.Finance.DataAccess.Entities;

namespace HotelLux.Finance.DataAccess.Repositories.Interfaces;

public interface IFacturaRepository
{
    Task<FacturaEntity?> ObtenerPorGuidAsync(Guid facturaGuid, CancellationToken ct = default);
    Task<FacturaEntity?> ObtenerParaActualizarPorGuidAsync(Guid facturaGuid, CancellationToken ct = default);
    Task<FacturaEntity?> ObtenerParaActualizarPorIdAsync(int idFactura, CancellationToken ct = default);
    Task<IReadOnlyList<FacturaEntity>> ListarAsync(Guid? clienteGuid, Guid? sucursalGuid, string? estado, CancellationToken ct = default);
    Task<IReadOnlyList<FacturaEntity>> ListarPorReservaGuidAsync(Guid reservaGuid, CancellationToken ct = default);
    Task<int> ContarPorTipoAnioAsync(string tipoFactura, int anio, CancellationToken ct = default);
    Task AgregarAsync(FacturaEntity entity, CancellationToken ct = default);
    void Actualizar(FacturaEntity entity);
}
