namespace HotelLux.Reservation.Business.DTOs.Reserva;

/// <summary>Contrato POST /api/v1/accommodations/reservas (endpoints_publicas.txt).</summary>
public class CrearReservaPublicRequest
{
    public Guid SucursalGuid { get; set; }
    public DateTimeOffset FechaInicio { get; set; }
    public DateTimeOffset FechaFin { get; set; }
    public string? OrigenCanalReserva { get; set; }
    public string? Observaciones { get; set; }
    public bool EsWalkin { get; set; }
    public ClienteInlineDTO? Cliente { get; set; }
    public List<ReservaHabitacionPublicRequest> Habitaciones { get; set; } = new();
}

public class ReservaHabitacionPublicRequest
{
    public Guid TipoHabitacionGuid { get; set; }
    public int NumHabitaciones { get; set; }
    public int NumAdultos { get; set; }
    public int NumNinos { get; set; }
}
