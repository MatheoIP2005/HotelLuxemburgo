using HotelLux.Finance.DataAccess.Entities;

namespace HotelLux.Finance.DataAccess.Repositories.Interfaces;

public interface IPagoRepository
{
    Task<PagoEntity?> ObtenerPorGuidAsync(Guid pagoGuid, CancellationToken ct = default);
    Task<PagoEntity?> ObtenerParaActualizarAsync(Guid pagoGuid, CancellationToken ct = default);
    Task<IReadOnlyList<PagoEntity>> ListarPorFacturaGuidAsync(Guid facturaGuid, CancellationToken ct = default);
    Task<IReadOnlyList<PagoEntity>> ListarFiltradoAsync(
        Guid? facturaGuid,
        Guid? reservaGuid,
        string? estadoPago,
        string? metodoPago,
        DateTimeOffset? fechaDesde,
        DateTimeOffset? fechaHasta,
        int maxResults,
        CancellationToken ct = default);
    Task AgregarAsync(PagoEntity entity, CancellationToken ct = default);
    void Actualizar(PagoEntity entity);
}
