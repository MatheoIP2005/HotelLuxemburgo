namespace HotelLux.Reservation.Business.DTOs.Reserva;

public class ReservaFiltroDTO
{
    public Guid? ClienteGuid { get; set; }
    public Guid? SucursalGuid { get; set; }
    public string? EstadoReserva { get; set; }
    public DateOnly? FechaDesde { get; set; }
    public DateOnly? FechaHasta { get; set; }
    public string? OrigenCanal { get; set; }
    public int Pagina { get; set; } = 1;
    public int Limite { get; set; } = 20;
}
