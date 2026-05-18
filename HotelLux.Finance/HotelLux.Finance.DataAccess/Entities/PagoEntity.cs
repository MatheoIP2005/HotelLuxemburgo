namespace HotelLux.Finance.DataAccess.Entities;

public class PagoEntity
{
    public int IdPago { get; set; }
    public Guid PagoGuid { get; set; }
    public int IdFactura { get; set; }
    public Guid ReservaGuid { get; set; }
    public decimal Monto { get; set; }
    public string MetodoPago { get; set; } = null!;
    public bool EsPagoElectronico { get; set; }
    public string? ProveedorPasarela { get; set; }
    public string? TransaccionExterna { get; set; }
    public string? CodigoAutorizacion { get; set; }
    public string? Referencia { get; set; }
    public string EstadoPago { get; set; } = null!;
    public DateTimeOffset FechaPagoUtc { get; set; }
    public string Moneda { get; set; } = null!;
    public decimal TipoCambio { get; set; }
    public string? RespuestaPasarela { get; set; }
    public string CreadoPorUsuario { get; set; } = null!;
    public DateTimeOffset FechaRegistroUtc { get; set; }
    public string? ModificadoPorUsuario { get; set; }
    public DateTimeOffset? FechaModificacionUtc { get; set; }
    public string? ModificacionIp { get; set; }
    public string ServicioOrigen { get; set; } = null!;

    public FacturaEntity? Factura { get; set; }
}
