namespace HotelLux.Finance.DataAccess.Entities;

public class FacturaEntity
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
    public string Moneda { get; set; } = null!;
    public string? ObservacionesFactura { get; set; }
    public string? OrigenCanalFactura { get; set; }
    public string Estado { get; set; } = null!;
    public DateTimeOffset? FechaInhabilitacionUtc { get; set; }
    public bool EsEliminado { get; set; }
    public string CreadoPorUsuario { get; set; } = null!;
    public DateTimeOffset FechaRegistroUtc { get; set; }
    public string? ModificadoPorUsuario { get; set; }
    public DateTimeOffset? FechaModificacionUtc { get; set; }
    public string? ModificacionIp { get; set; }
    public string ServicioOrigen { get; set; } = null!;
    public string? MotivoInhabilitacion { get; set; }

    public ICollection<FacturaDetalleEntity> Detalles { get; set; } = new List<FacturaDetalleEntity>();
    public ICollection<PagoEntity> Pagos { get; set; } = new List<PagoEntity>();
}
