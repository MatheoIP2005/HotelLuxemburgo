using HotelLux.Finance.DataAccess.Entities;
using HotelLux.Finance.DataManagement.Interfaces;
using HotelLux.Finance.DataManagement.Mappers;
using HotelLux.Finance.DataManagement.Models;

namespace HotelLux.Finance.DataManagement.Services;

public class PagoDataService : IPagoDataService
{
    private readonly IUnitOfWork _uow;
    public PagoDataService(IUnitOfWork uow) => _uow = uow;

    public async Task<PagoDataModel?> ObtenerPorGuidAsync(Guid pagoGuid, CancellationToken ct = default)
    {
        var e = await _uow.PagoRepository.ObtenerPorGuidAsync(pagoGuid, ct);
        if (e?.Factura is null) return null;
        return FinanceDataMapper.ToPagoModel(e, e.Factura.FacturaGuid);
    }

    public async Task<IReadOnlyList<PagoDataModel>> ListarPorFacturaAsync(Guid facturaGuid, CancellationToken ct = default)
    {
        var list = await _uow.PagoRepository.ListarPorFacturaGuidAsync(facturaGuid, ct);
        return list.Select(p => FinanceDataMapper.ToPagoModel(p, facturaGuid)).ToList();
    }

    public async Task<IReadOnlyList<PagoDataModel>> ListarFiltradoAsync(
        Guid? facturaGuid,
        Guid? reservaGuid,
        string? estadoPago,
        string? metodoPago,
        DateTimeOffset? fechaDesde,
        DateTimeOffset? fechaHasta,
        int maxResults,
        CancellationToken ct = default)
    {
        var list = await _uow.PagoRepository.ListarFiltradoAsync(
            facturaGuid, reservaGuid, estadoPago, metodoPago, fechaDesde, fechaHasta, maxResults, ct);
        return list.Select(p =>
        {
            var fg = p.Factura?.FacturaGuid ?? Guid.Empty;
            return FinanceDataMapper.ToPagoModel(p, fg);
        }).ToList();
    }

    public async Task<PagoDataModel> RegistrarAsync(
        Guid facturaGuid, decimal monto, string metodoPago, string creadoPorUsuario, CancellationToken ct = default)
    {
        var factura = await _uow.FacturaRepository.ObtenerPorGuidAsync(facturaGuid, ct);
        if (factura is null)
            throw new InvalidOperationException("Factura no encontrada.");

        var entity = new PagoEntity
        {
            PagoGuid = Guid.NewGuid(),
            IdFactura = factura.IdFactura,
            ReservaGuid = factura.ReservaGuid,
            Monto = monto,
            MetodoPago = metodoPago,
            EsPagoElectronico = metodoPago.Contains("TARJETA", StringComparison.OrdinalIgnoreCase),
            EstadoPago = "PEN",
            FechaPagoUtc = DateTimeOffset.UtcNow,
            Moneda = "USD",
            TipoCambio = 1m,
            CreadoPorUsuario = creadoPorUsuario,
            FechaRegistroUtc = DateTimeOffset.UtcNow,
            ServicioOrigen = "finance-service"
        };

        await _uow.PagoRepository.AgregarAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return FinanceDataMapper.ToPagoModel(entity, factura.FacturaGuid);
    }

    public async Task<bool> AprobarAsync(Guid pagoGuid, string usuario, CancellationToken ct = default)
    {
        var pago = await _uow.PagoRepository.ObtenerParaActualizarAsync(pagoGuid, ct);
        if (pago is null || pago.EstadoPago != "PEN") return false;

        var factura = await _uow.FacturaRepository.ObtenerParaActualizarPorIdAsync(pago.IdFactura, ct);
        if (factura is null || factura.Estado == "ANU") return false;

        pago.EstadoPago = "APR";
        pago.ModificadoPorUsuario = usuario;
        pago.FechaModificacionUtc = DateTimeOffset.UtcNow;

        factura.SaldoPendiente = Math.Max(0, factura.SaldoPendiente - pago.Monto);
        if (factura.SaldoPendiente == 0) factura.Estado = "PAG";
        factura.ModificadoPorUsuario = usuario;
        factura.FechaModificacionUtc = DateTimeOffset.UtcNow;

        _uow.PagoRepository.Actualizar(pago);
        _uow.FacturaRepository.Actualizar(factura);
        await _uow.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> ActualizarEstadoAsync(Guid pagoGuid, string estado, string usuario, CancellationToken ct = default)
    {
        var normalized = estado.Trim().ToUpperInvariant();
        if (normalized is not ("APR" or "REC" or "CAN" or "PEN"))
            throw new InvalidOperationException("Estado de pago no válido. Use PEN, APR, REC o CAN.");

        if (normalized == "APR")
            return await AprobarAsync(pagoGuid, usuario, ct);

        var pago = await _uow.PagoRepository.ObtenerParaActualizarAsync(pagoGuid, ct);
        if (pago is null) return false;

        pago.EstadoPago = normalized;
        pago.ModificadoPorUsuario = usuario;
        pago.FechaModificacionUtc = DateTimeOffset.UtcNow;
        _uow.PagoRepository.Actualizar(pago);
        await _uow.SaveChangesAsync(ct);
        return true;
    }
}
