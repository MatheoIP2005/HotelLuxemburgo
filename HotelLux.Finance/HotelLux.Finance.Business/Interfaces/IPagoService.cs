using HotelLux.Finance.DataManagement.Models;

namespace HotelLux.Finance.Business.Interfaces;

public interface IPagoService
{
    Task<PagoDataModel?> ObtenerPorGuidAsync(Guid pagoGuid, CancellationToken ct = default);
    Task<IReadOnlyList<PagoDataModel>> ListarPorFacturaAsync(Guid facturaGuid, CancellationToken ct = default);
    Task<IReadOnlyList<PagoDataModel>> ListarFiltradoAsync(
        Guid? facturaGuid,
        Guid? reservaGuid,
        string? estadoPago,
        string? metodoPago,
        DateTimeOffset? fechaDesde,
        DateTimeOffset? fechaHasta,
        int maxResults,
        CancellationToken ct = default);
    Task<PagoDataModel> RegistrarAsync(Guid facturaGuid, decimal monto, string metodoPago, string creadoPorUsuario, CancellationToken ct = default);
    Task<bool> AprobarAsync(Guid pagoGuid, string usuario, CancellationToken ct = default);
    Task<bool> ActualizarEstadoAsync(Guid pagoGuid, string estado, string usuario, CancellationToken ct = default);
}
