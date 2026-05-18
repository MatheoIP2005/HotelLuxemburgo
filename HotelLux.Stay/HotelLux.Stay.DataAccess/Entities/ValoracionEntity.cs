namespace HotelLux.Stay.DataAccess.Entities;

public class ValoracionEntity
{
    public int IdValoracion { get; set; }
    public Guid ValoracionGuid { get; set; }
    public Guid EstadiaGuid { get; set; }
    public Guid SucursalGuid { get; set; }
    public Guid ClienteGuid { get; set; }
    public decimal PuntuacionGeneral { get; set; }
    public decimal PuntuacionLimpieza { get; set; }
    public decimal PuntuacionConfort { get; set; }
    public decimal PuntuacionUbicacion { get; set; }
    public decimal PuntuacionInstalaciones { get; set; }
    public decimal PuntuacionPersonal { get; set; }
    public decimal PuntuacionCalidadPrecio { get; set; }
    public string ComentarioPositivo { get; set; } = null!;
    public string ComentarioNegativo { get; set; } = null!;
    public string TipoViaje { get; set; } = null!;
    public DateTimeOffset FechaPublicacionUtc { get; set; }
    public string? RespuestaHotel { get; set; }
    public string? NombreVisibleCliente { get; set; }
    public bool EsEliminado { get; set; }
    public DateTimeOffset FechaRegistroUtc { get; set; }
    public string CreadoPorUsuario { get; set; } = null!;
}
