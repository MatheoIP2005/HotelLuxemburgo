namespace HotelLux.Stay.Business.DTOs;

public class CheckInDto
{
    public Guid ReservaGuid { get; set; }
    public Guid? ReservaHabitacionGuid { get; set; }
    public string? ObservacionesCheckin { get; set; }
    public string? CreadoPorUsuario { get; set; }
}

/// <summary>Cuerpo opcional para el alias POST .../estadias/checkin/{reservaGuid}.</summary>
public class CheckInPorReservaBodyDto
{
    public Guid? ReservaHabitacionGuid { get; set; }
    public string? ObservacionesCheckin { get; set; }
}

public class CheckoutPorBodyDto
{
    public Guid EstadiaGuid { get; set; }
}

public class EstadiaDto
{
    public Guid EstadiaGuid { get; set; }
    public Guid ReservaGuid { get; set; }
    public Guid ReservaHabitacionGuid { get; set; }
    public Guid ClienteGuid { get; set; }
    public Guid SucursalGuid { get; set; }
    public Guid HabitacionGuid { get; set; }
    public string Estado { get; set; } = null!;
    public DateTimeOffset? FechaCheckinUtc { get; set; }
    public DateTimeOffset? FechaCheckoutUtc { get; set; }
    public bool RequiereMantenimiento { get; set; }
}
