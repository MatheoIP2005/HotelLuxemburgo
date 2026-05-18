namespace HotelLux.Finance.DataAccess.Entities;

public class FacturaDetalleEntity
{
    public int IdFacturaDetalle { get; set; }
    public Guid FacturaDetalleGuid { get; set; }
    public int IdFactura { get; set; }
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
    public DateTimeOffset FechaRegistroUtc { get; set; }
    public string CreadoPorUsuario { get; set; } = null!;

    public FacturaEntity? Factura { get; set; }
}
