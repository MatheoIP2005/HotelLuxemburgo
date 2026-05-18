using HotelLux.Finance.Business.DTOs;
using HotelLux.Finance.Business.Exceptions;
using HotelLux.Finance.Business.Interfaces;
using HotelLux.Finance.DataManagement.Interfaces;
using HotelLux.Finance.DataManagement.Models;

namespace HotelLux.Finance.Business.Services;

public class FacturaService : IFacturaService
{
    private readonly IFacturaDataService _data;
    public FacturaService(IFacturaDataService data) => _data = data;

    public async Task<FacturaDto?> ObtenerPorGuidAsync(Guid facturaGuid, CancellationToken ct = default)
    {
        var m = await _data.ObtenerPorGuidAsync(facturaGuid, ct);
        return m is null ? null : ToDto(m);
    }

    public async Task<IReadOnlyList<FacturaDto>> ListarAsync(
        Guid? clienteGuid, Guid? sucursalGuid, string? estado, CancellationToken ct = default)
    {
        var list = await _data.ListarAsync(clienteGuid, sucursalGuid, estado, ct);
        return list.Select(ToDto).ToList();
    }

    public Task<FacturaDataModel> GenerarAsync(FacturaDataModel model, CancellationToken ct = default)
        => _data.CrearAsync(model, ct);

    public Task<bool> ActualizarSaldoAsync(Guid facturaGuid, decimal montoPagado, CancellationToken ct = default)
        => _data.ActualizarSaldoAsync(facturaGuid, montoPagado, ct);

    public Task<bool> MarcarPagadaAsync(Guid facturaGuid, CancellationToken ct = default)
        => _data.MarcarPagadaAsync(facturaGuid, ct);

    public async Task<IReadOnlyList<FacturaDetalleDataModel>> ListarDetallesAsync(Guid facturaGuid, CancellationToken ct = default)
    {
        var factura = await _data.ObtenerPorGuidAsync(facturaGuid, ct);
        if (factura is null) throw new NotFoundException("Factura", facturaGuid);
        return factura.Detalles;
    }

    public Task<bool> AnularAsync(Guid facturaGuid, string motivo, string usuario, CancellationToken ct = default)
        => _data.AnularAsync(facturaGuid, motivo, usuario, ct);

    public async Task<IReadOnlyList<FacturaDto>> ListarPorReservaGuidAsync(Guid reservaGuid, CancellationToken ct = default)
    {
        var list = await _data.ListarPorReservaGuidAsync(reservaGuid, ct);
        return list.Select(ToDto).ToList();
    }

    public async Task<FacturaDto> GenerarConLineasAsync(
        string tipoFactura,
        Guid reservaGuid,
        Guid clienteGuid,
        Guid sucursalGuid,
        IReadOnlyList<FacturaLineaGeneracionDto> lineas,
        string usuario,
        CancellationToken ct = default)
    {
        if (lineas.Count == 0)
            throw new ValidationException("Se requiere al menos una línea de detalle.",
                new[] { "Items no puede estar vacío." });

        if (string.Equals(tipoFactura, "RESERVA", StringComparison.OrdinalIgnoreCase))
        {
            var prev = await _data.ListarPorReservaGuidAsync(reservaGuid, ct);
            foreach (var f in prev.Where(x =>
                         string.Equals(x.TipoFactura, "RESERVA", StringComparison.OrdinalIgnoreCase)))
            {
                if (string.Equals(f.Estado, "ANU", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!string.Equals(f.Estado, "EMI", StringComparison.OrdinalIgnoreCase))
                    throw new ValidationException(
                        "No se puede regenerar la factura de reserva mientras exista una factura en un estado distinto de emitida (EMI).",
                        new[] { $"Factura {f.NumeroFactura} en estado {f.Estado}." });

                if (f.Total > 0 && f.SaldoPendiente < f.Total)
                    throw new ValidationException(
                        "No se puede regenerar la factura de reserva: hay pagos o abonos registrados contra el saldo.",
                        new[] { $"Factura {f.NumeroFactura} con saldo pendiente distinto al total." });

                await _data.AnularAsync(
                    f.FacturaGuid,
                    "Reemplazada por nueva factura de reserva (actualización de líneas).",
                    usuario,
                    ct);
            }
        }

        var detalles = lineas.Select(i => new FacturaDetalleDataModel
        {
            TipoItem = string.IsNullOrWhiteSpace(i.TipoItem)
                ? (tipoFactura == "FINAL" ? "SERVICIO" : "ALOJAMIENTO")
                : i.TipoItem!,
            ReferenciaTipo = i.ReferenciaTipo,
            ReferenciaGuid = i.ReferenciaGuid,
            DescripcionItem = i.Descripcion,
            Cantidad = i.Cantidad <= 0 ? 1 : i.Cantidad,
            PrecioUnitario = i.PrecioUnitario,
            SubtotalLinea = i.Subtotal,
            ValorIvaLinea = i.ValorIva,
            DescuentoLinea = i.Descuento,
            TotalLinea = i.Total
        }).ToList();

        var subtotal = detalles.Sum(x => x.SubtotalLinea);
        var iva = detalles.Sum(x => x.ValorIvaLinea);
        var descuento = detalles.Sum(x => x.DescuentoLinea);
        var total = detalles.Sum(x => x.TotalLinea);

        var model = new FacturaDataModel
        {
            ReservaGuid = reservaGuid,
            ClienteGuid = clienteGuid,
            SucursalGuid = sucursalGuid,
            TipoFactura = tipoFactura,
            Subtotal = subtotal,
            ValorIva = iva,
            DescuentoTotal = descuento,
            Total = total,
            SaldoPendiente = total,
            Estado = "EMI",
            CreadoPorUsuario = string.IsNullOrWhiteSpace(usuario) ? "finance_api" : usuario,
            Detalles = detalles
        };

        var created = await _data.CrearAsync(model, ct);
        return ToDto(created);
    }

    private static FacturaDto ToDto(FacturaDataModel m) => new()
    {
        FacturaGuid = m.FacturaGuid,
        ClienteGuid = m.ClienteGuid,
        ReservaGuid = m.ReservaGuid,
        SucursalGuid = m.SucursalGuid,
        NumeroFactura = m.NumeroFactura,
        TipoFactura = m.TipoFactura,
        Total = m.Total,
        SaldoPendiente = m.SaldoPendiente,
        Estado = m.Estado
    };
}
