using Grpc.Core;
using HotelLux.Finance.Business.DTOs;
using HotelLux.Finance.Business.Interfaces;
using HotelLux.Protos.Finance;

namespace HotelLux.Finance.API.GrpcServices;

public class FinanceGrpcService : FinanceService.FinanceServiceBase
{
    private readonly IFacturaService _facturas;

    public FinanceGrpcService(IFacturaService facturas) => _facturas = facturas;

    public override async Task<GenerateInvoiceResponse> GenerateReservationInvoice(
        GenerateReservationInvoiceRequest request, ServerCallContext context)
        => await GenerarFacturaAsync("RESERVA", request.ReservaGuid,
            request.ClienteGuid, request.SucursalGuid, request.Items, request.CreadoPorUsuario, context.CancellationToken);

    public override async Task<GenerateInvoiceResponse> GenerateFinalInvoice(
        GenerateFinalInvoiceRequest request, ServerCallContext context)
        => await GenerarFacturaAsync("FINAL", request.ReservaGuid,
            request.ClienteGuid, request.SucursalGuid, request.Items, request.CreadoPorUsuario, context.CancellationToken);

    public override async Task<UpdateInvoiceBalanceResponse> UpdateInvoiceBalance(
        UpdateInvoiceBalanceRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.FacturaGuid, out var facturaGuid))
            return new UpdateInvoiceBalanceResponse { Success = false, Mensaje = "FacturaGuid inválido." };
        var ok = await _facturas.ActualizarSaldoAsync(facturaGuid, (decimal)request.MontoPagado, context.CancellationToken);
        var factura = ok ? await _facturas.ObtenerPorGuidAsync(facturaGuid, context.CancellationToken) : null;
        return new UpdateInvoiceBalanceResponse
        {
            Success = ok,
            SaldoPendiente = factura is null ? 0 : (double)factura.SaldoPendiente,
            Mensaje = ok ? "Saldo actualizado." : "Factura no encontrada."
        };
    }

    public override async Task<MarkInvoicePaidResponse> MarkInvoicePaid(
        MarkInvoicePaidRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.FacturaGuid, out var facturaGuid))
            return new MarkInvoicePaidResponse { Success = false, Mensaje = "FacturaGuid inválido." };
        var ok = await _facturas.MarcarPagadaAsync(facturaGuid, context.CancellationToken);
        return new MarkInvoicePaidResponse
        {
            Success = ok,
            Mensaje = ok ? "Factura pagada." : "Factura no encontrada."
        };
    }

    private static IReadOnlyList<FacturaLineaGeneracionDto> MapProtoItems(IEnumerable<InvoiceLineItem> items)
        => items.Select(i => new FacturaLineaGeneracionDto
        {
            Descripcion = i.Descripcion,
            Cantidad = i.Cantidad,
            PrecioUnitario = (decimal)i.PrecioUnitario,
            Subtotal = (decimal)i.Subtotal,
            ValorIva = (decimal)i.ValorIva,
            Descuento = (decimal)i.Descuento,
            Total = (decimal)i.Total,
            TipoItem = string.IsNullOrWhiteSpace(i.TipoItem) ? null : i.TipoItem,
            ReferenciaTipo = string.IsNullOrWhiteSpace(i.ReferenciaTipo) ? null : i.ReferenciaTipo,
            ReferenciaGuid = Guid.TryParse(i.ReferenciaGuid, out var rg) ? rg : null
        }).ToList();

    private async Task<GenerateInvoiceResponse> GenerarFacturaAsync(
        string tipo,
        string reservaGuidText,
        string clienteGuidText,
        string sucursalGuidText,
        IEnumerable<InvoiceLineItem> items,
        string creadoPorUsuario,
        CancellationToken ct)
    {
        if (!Guid.TryParse(reservaGuidText, out var reservaGuid) ||
            !Guid.TryParse(clienteGuidText, out var clienteGuid) ||
            !Guid.TryParse(sucursalGuidText, out var sucursalGuid))
        {
            return new GenerateInvoiceResponse { Success = false, Mensaje = "GUIDs inválidos." };
        }

        try
        {
            var lineas = MapProtoItems(items);
            var usuario = string.IsNullOrWhiteSpace(creadoPorUsuario) ? "finance_api" : creadoPorUsuario;
            var dto = await _facturas.GenerarConLineasAsync(
                tipo, reservaGuid, clienteGuid, sucursalGuid, lineas, usuario, ct);
            return new GenerateInvoiceResponse
            {
                Success = true,
                FacturaGuid = dto.FacturaGuid.ToString(),
                NumeroFactura = dto.NumeroFactura,
                Total = (double)dto.Total,
                Mensaje = "Factura generada."
            };
        }
        catch (HotelLux.Finance.Business.Exceptions.ValidationException ex)
        {
            return new GenerateInvoiceResponse { Success = false, Mensaje = ex.Message };
        }
    }
}
