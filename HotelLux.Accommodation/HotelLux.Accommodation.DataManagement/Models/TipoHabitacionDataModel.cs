namespace HotelLux.Accommodation.DataManagement.Models;

public class TipoHabitacionDataModel
{
    public int IdTipoHabitacion { get; set; }
    public Guid TipoHabitacionGuid { get; set; }
    public string CodigoTipoHabitacion { get; set; } = null!;
    public string NombreTipoHabitacion { get; set; } = null!;
    public string? Descripcion { get; set; }
    public int CapacidadAdultos { get; set; }
    public int CapacidadNinos { get; set; }
    public int CapacidadTotal { get; set; }
    public string? TipoCama { get; set; }
    public decimal? AreaM2 { get; set; }
    public bool PermiteEventos { get; set; }
    public bool PermiteReservaPublica { get; set; }
    public string EstadoTipoHabitacion { get; set; } = null!;
    public bool EsEliminado { get; set; }
    public DateTimeOffset? FechaInhabilitacionUtc { get; set; }
    public string? MotivoInhabilitacion { get; set; }
    public DateTimeOffset FechaRegistroUtc { get; set; }
    public string CreadoPorUsuario { get; set; } = null!;
    public string? ModificadoPorUsuario { get; set; }
    public DateTimeOffset? FechaModificacionUtc { get; set; }
    public string? ModificacionIp { get; set; }
    public string ServicioOrigen { get; set; } = null!;
}
