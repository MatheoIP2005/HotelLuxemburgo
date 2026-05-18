namespace HotelLux.Accommodation.DataManagement.Models;

public class HabitacionDataModel
{
    public int IdHabitacion { get; set; }
    public Guid HabitacionGuid { get; set; }
    public int IdSucursal { get; set; }
    public int IdTipoHabitacion { get; set; }
    public Guid SucursalGuid { get; set; }
    public Guid TipoHabitacionGuid { get; set; }
    public string NumeroHabitacion { get; set; } = null!;
    public int? Piso { get; set; }
    public int CapacidadHabitacion { get; set; }
    public decimal PrecioBase { get; set; }
    public string? DescripcionHabitacion { get; set; }
    public string EstadoHabitacion { get; set; } = null!;
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
