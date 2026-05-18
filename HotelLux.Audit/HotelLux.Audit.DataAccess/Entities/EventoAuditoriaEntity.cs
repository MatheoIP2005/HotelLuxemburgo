namespace HotelLux.Audit.DataAccess.Entities;

public class EventoAuditoriaEntity
{
    public long IdAuditoria { get; set; }
    public Guid AuditoriaGuid { get; set; }
    public string TablaAfectada { get; set; } = null!;
    public string Operacion { get; set; } = null!;
    public Guid? EntidadGuid { get; set; }
    public string? IdRegistroAfectado { get; set; }
    public string? DatosAnteriores { get; set; }
    public string? DatosNuevos { get; set; }
    public string UsuarioEjecutor { get; set; } = null!;
    public Guid? UsuarioGuid { get; set; }
    public string? IpOrigen { get; set; }
    public string ServicioOrigen { get; set; } = null!;
    public DateTimeOffset FechaEventoUtc { get; set; }
    public bool Activo { get; set; }
}
