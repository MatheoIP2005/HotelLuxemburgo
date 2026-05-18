using HotelLux.Finance.Business.Exceptions;
using HotelLux.Finance.Business.Interfaces;
using HotelLux.Finance.DataManagement.Interfaces;
using HotelLux.Finance.DataManagement.Models;

namespace HotelLux.Finance.Business.Services;

public class PagoService : IPagoService
{
    private readonly IPagoDataService _data;
    private readonly IFacturaDataService _facturas;

    public PagoService(IPagoDataService data, IFacturaDataService facturas)
    {
        _data = data;
        _facturas = facturas;
    }

    public Task<PagoDataModel?> ObtenerPorGuidAsync(Guid pagoGuid, CancellationToken ct = default)
        => _data.ObtenerPorGuidAsync(pagoGuid, ct);

    public Task<IReadOnlyList<PagoDataModel>> ListarPorFacturaAsync(Guid facturaGuid, CancellationToken ct = default)
        => _data.ListarPorFacturaAsync(facturaGuid, ct);

    public Task<IReadOnlyList<PagoDataModel>> ListarFiltradoAsync(
        Guid? facturaGuid,
        Guid? reservaGuid,
        string? estadoPago,
        string? metodoPago,
        DateTimeOffset? fechaDesde,
        DateTimeOffset? fechaHasta,
        int maxResults,
        CancellationToken ct = default)
        => _data.ListarFiltradoAsync(
            facturaGuid, reservaGuid, estadoPago, metodoPago, fechaDesde, fechaHasta, maxResults, ct);

    public async Task<PagoDataModel> RegistrarAsync(
        Guid facturaGuid, decimal monto, string metodoPago, string creadoPorUsuario, CancellationToken ct = default)
    {
        var f = await _facturas.ObtenerPorGuidAsync(facturaGuid, ct);
        if (f is null) throw new NotFoundException("Factura", facturaGuid);
        return await _data.RegistrarAsync(facturaGuid, monto, metodoPago, creadoPorUsuario, ct);
    }

    public Task<bool> AprobarAsync(Guid pagoGuid, string usuario, CancellationToken ct = default)
        => _data.AprobarAsync(pagoGuid, usuario, ct);

    public Task<bool> ActualizarEstadoAsync(Guid pagoGuid, string estado, string usuario, CancellationToken ct = default)
        => _data.ActualizarEstadoAsync(pagoGuid, estado, usuario, ct);
}
