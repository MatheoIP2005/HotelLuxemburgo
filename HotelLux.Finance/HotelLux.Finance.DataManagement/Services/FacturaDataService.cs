using HotelLux.Finance.DataAccess.Entities;
using HotelLux.Finance.DataManagement.Interfaces;
using HotelLux.Finance.DataManagement.Mappers;
using HotelLux.Finance.DataManagement.Models;

namespace HotelLux.Finance.DataManagement.Services;

public class FacturaDataService : IFacturaDataService
{
    private readonly IUnitOfWork _uow;
    public FacturaDataService(IUnitOfWork uow) => _uow = uow;

    public async Task<FacturaDataModel?> ObtenerPorGuidAsync(Guid facturaGuid, CancellationToken ct = default)
    {
        var e = await _uow.FacturaRepository.ObtenerPorGuidAsync(facturaGuid, ct);
        return e is null ? null : FinanceDataMapper.ToModel(e);
    }

    public async Task<IReadOnlyList<FacturaDataModel>> ListarAsync(
        Guid? clienteGuid, Guid? sucursalGuid, string? estado, CancellationToken ct = default)
    {
        var list = await _uow.FacturaRepository.ListarAsync(clienteGuid, sucursalGuid, estado, ct);
        return list.Select(FinanceDataMapper.ToModel).ToList();
    }

    public async Task<IReadOnlyList<FacturaDataModel>> ListarPorReservaGuidAsync(Guid reservaGuid, CancellationToken ct = default)
    {
        var list = await _uow.FacturaRepository.ListarPorReservaGuidAsync(reservaGuid, ct);
        return list.Select(FinanceDataMapper.ToModel).ToList();
    }

    public async Task<FacturaDataModel> CrearAsync(FacturaDataModel model, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        var seq = await _uow.FacturaRepository.ContarPorTipoAnioAsync(model.TipoFactura, now.Year, ct) + 1;
        var prefix = model.TipoFactura == "FINAL" ? "FAC-FIN" : "FAC-RES";
        var e = new FacturaEntity
        {
            FacturaGuid = model.FacturaGuid == Guid.Empty ? Guid.NewGuid() : model.FacturaGuid,
            ClienteGuid = model.ClienteGuid,
            ReservaGuid = model.ReservaGuid,
            SucursalGuid = model.SucursalGuid,
            NumeroFactura = string.IsNullOrWhiteSpace(model.NumeroFactura)
                ? $"{prefix}-{now:yyyy}-{seq:000000}"
                : model.NumeroFactura,
            TipoFactura = model.TipoFactura,
            FechaEmision = now,
            Subtotal = model.Subtotal,
            ValorIva = model.ValorIva,
            DescuentoTotal = model.DescuentoTotal,
            Total = model.Total,
            SaldoPendiente = model.SaldoPendiente,
            Moneda = "USD",
            Estado = model.Estado,
            CreadoPorUsuario = model.CreadoPorUsuario,
            FechaRegistroUtc = now,
            ServicioOrigen = "finance-service",
            Detalles = model.Detalles.Select(d => new FacturaDetalleEntity
            {
                FacturaDetalleGuid = d.FacturaDetalleGuid == Guid.Empty ? Guid.NewGuid() : d.FacturaDetalleGuid,
                TipoItem = d.TipoItem,
                ReferenciaTipo = d.ReferenciaTipo,
                ReferenciaGuid = d.ReferenciaGuid,
                DescripcionItem = d.DescripcionItem,
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario,
                SubtotalLinea = d.SubtotalLinea,
                ValorIvaLinea = d.ValorIvaLinea,
                DescuentoLinea = d.DescuentoLinea,
                TotalLinea = d.TotalLinea,
                CreadoPorUsuario = model.CreadoPorUsuario,
                FechaRegistroUtc = now
            }).ToList()
        };

        await _uow.FacturaRepository.AgregarAsync(e, ct);
        await _uow.SaveChangesAsync(ct);
        var saved = await _uow.FacturaRepository.ObtenerPorGuidAsync(e.FacturaGuid, ct) ?? e;
        return FinanceDataMapper.ToModel(saved);
    }

    public async Task<bool> ActualizarSaldoAsync(Guid facturaGuid, decimal montoPagado, CancellationToken ct = default)
    {
        var e = await _uow.FacturaRepository.ObtenerParaActualizarPorGuidAsync(facturaGuid, ct);
        if (e is null) return false;
        e.SaldoPendiente = Math.Max(0, e.SaldoPendiente - montoPagado);
        if (e.SaldoPendiente == 0) e.Estado = "PAG";
        e.FechaModificacionUtc = DateTimeOffset.UtcNow;
        _uow.FacturaRepository.Actualizar(e);
        await _uow.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> MarcarPagadaAsync(Guid facturaGuid, CancellationToken ct = default)
    {
        var e = await _uow.FacturaRepository.ObtenerParaActualizarPorGuidAsync(facturaGuid, ct);
        if (e is null) return false;
        e.Estado = "PAG";
        e.SaldoPendiente = 0;
        e.FechaModificacionUtc = DateTimeOffset.UtcNow;
        _uow.FacturaRepository.Actualizar(e);
        await _uow.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> AnularAsync(Guid facturaGuid, string motivo, string usuario, CancellationToken ct = default)
    {
        var e = await _uow.FacturaRepository.ObtenerParaActualizarPorGuidAsync(facturaGuid, ct);
        if (e is null || e.Estado == "ANU") return false;
        e.Estado = "ANU";
        e.MotivoInhabilitacion = motivo;
        e.FechaInhabilitacionUtc = DateTimeOffset.UtcNow;
        e.ModificadoPorUsuario = usuario;
        e.FechaModificacionUtc = DateTimeOffset.UtcNow;
        _uow.FacturaRepository.Actualizar(e);
        await _uow.SaveChangesAsync(ct);
        return true;
    }
}
