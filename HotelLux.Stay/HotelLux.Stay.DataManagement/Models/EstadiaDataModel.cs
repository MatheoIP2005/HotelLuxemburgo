namespace HotelLux.Stay.DataManagement.Models;

public class EstadiaDataModel
{
    public int IdEstadia { get; set; }
    public Guid EstadiaGuid { get; set; }
    public Guid ReservaGuid { get; set; }
    public Guid ReservaHabitacionGuid { get; set; }
    public Guid ClienteGuid { get; set; }
    public Guid SucursalGuid { get; set; }
    public Guid HabitacionGuid { get; set; }
    public string Estado { get; set; } = null!;
    public DateTimeOffset? FechaCheckinUtc { get; set; }
    public DateTimeOffset? FechaCheckoutUtc { get; set; }
    public string? ObservacionesCheckin { get; set; }
    public string? ObservacionesCheckout { get; set; }
    public bool RequiereMantenimiento { get; set; }
    public DateTimeOffset FechaRegistroUtc { get; set; }
    public string CreadoPorUsuario { get; set; } = null!;
    public string? ModificadoPorUsuario { get; set; }
    public DateTimeOffset? FechaModificacionUtc { get; set; }
    public bool EsEliminado { get; set; }
    public string ServicioOrigen { get; set; } = null!;
}
