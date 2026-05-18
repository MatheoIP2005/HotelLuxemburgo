namespace HotelLux.Finance.DataManagement.Models;

public class FacturaDataModel
{
    public int IdFactura { get; set; }
    public Guid FacturaGuid { get; set; }
    public Guid ClienteGuid { get; set; }
    public Guid ReservaGuid { get; set; }
    public Guid SucursalGuid { get; set; }
    public string NumeroFactura { get; set; } = null!;
    public string TipoFactura { get; set; } = null!;
    public DateTimeOffset FechaEmision { get; set; }
    public decimal Subtotal { get; set; }
    public decimal ValorIva { get; set; }
    public decimal DescuentoTotal { get; set; }
    public decimal Total { get; set; }
    public decimal SaldoPendiente { get; set; }
    public string Estado { get; set; } = null!;
    public string CreadoPorUsuario { get; set; } = null!;
    public List<FacturaDetalleDataModel> Detalles { get; set; } = new();
}

public class FacturaDetalleDataModel
{
    public Guid FacturaDetalleGuid { get; set; }
    public string TipoItem { get; set; } = null!;
    public string? ReferenciaTipo { get; set; }
    public Guid? ReferenciaGuid { get; set; }
    public string DescripcionItem { get; set; } = null!;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal SubtotalLinea { get; set; }
    public decimal ValorIvaLinea { get; set; }
    public decimal DescuentoLinea { get; set; }
    public decimal TotalLinea { get; set; }
}

public class PagoDataModel
{
    public Guid PagoGuid { get; set; }
    public Guid FacturaGuid { get; set; }
    public Guid ReservaGuid { get; set; }
    public decimal Monto { get; set; }
    public string MetodoPago { get; set; } = null!;
    public string EstadoPago { get; set; } = null!;
    public string CreadoPorUsuario { get; set; } = null!;
}
