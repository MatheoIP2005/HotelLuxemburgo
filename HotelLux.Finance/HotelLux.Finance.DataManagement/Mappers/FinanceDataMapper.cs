using HotelLux.Finance.DataAccess.Entities;
using HotelLux.Finance.DataManagement.Models;

namespace HotelLux.Finance.DataManagement.Mappers;

public static class FinanceDataMapper
{
    public static FacturaDataModel ToModel(FacturaEntity e) => new()
    {
        IdFactura = e.IdFactura,
        FacturaGuid = e.FacturaGuid,
        ClienteGuid = e.ClienteGuid,
        ReservaGuid = e.ReservaGuid,
        SucursalGuid = e.SucursalGuid,
        NumeroFactura = e.NumeroFactura,
        TipoFactura = e.TipoFactura,
        FechaEmision = e.FechaEmision,
        Subtotal = e.Subtotal,
        ValorIva = e.ValorIva,
        DescuentoTotal = e.DescuentoTotal,
        Total = e.Total,
        SaldoPendiente = e.SaldoPendiente,
        Estado = e.Estado,
        CreadoPorUsuario = e.CreadoPorUsuario,
        Detalles = e.Detalles.Select(ToModel).ToList()
    };

    public static FacturaDetalleDataModel ToModel(FacturaDetalleEntity e) => new()
    {
        FacturaDetalleGuid = e.FacturaDetalleGuid,
        TipoItem = e.TipoItem,
        ReferenciaTipo = e.ReferenciaTipo,
        ReferenciaGuid = e.ReferenciaGuid,
        DescripcionItem = e.DescripcionItem,
        Cantidad = e.Cantidad,
        PrecioUnitario = e.PrecioUnitario,
        SubtotalLinea = e.SubtotalLinea,
        ValorIvaLinea = e.ValorIvaLinea,
        DescuentoLinea = e.DescuentoLinea,
        TotalLinea = e.TotalLinea
    };

    public static FacturaDetalleDataModel ToDetalleModel(FacturaDetalleEntity e) => ToModel(e);

    public static PagoDataModel ToPagoModel(PagoEntity e, Guid facturaGuid) => new()
    {
        PagoGuid = e.PagoGuid,
        FacturaGuid = facturaGuid,
        ReservaGuid = e.ReservaGuid,
        Monto = e.Monto,
        MetodoPago = e.MetodoPago,
        EstadoPago = e.EstadoPago,
        CreadoPorUsuario = e.CreadoPorUsuario
    };
}
