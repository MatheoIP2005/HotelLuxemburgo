namespace HotelLux.Reservation.Business.DTOs.ReservaHabitacion;

public class ReservaHabitacionDTO
{
    public Guid ReservaHabitacionGuid { get; set; }
    public Guid HabitacionGuid { get; set; }
    public Guid? TarifaGuid { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public int NumAdultos { get; set; }
    public int NumNinos { get; set; }
    public decimal PrecioNocheAplicado { get; set; }
    public decimal SubtotalLinea { get; set; }
    public decimal ValorIvaLinea { get; set; }
    public decimal DescuentoLinea { get; set; }
    public decimal TotalLinea { get; set; }
    public string EstadoDetalle { get; set; } = null!;
}
