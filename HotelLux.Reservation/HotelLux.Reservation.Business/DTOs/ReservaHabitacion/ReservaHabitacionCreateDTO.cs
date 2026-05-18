namespace HotelLux.Reservation.Business.DTOs.ReservaHabitacion;

public class ReservaHabitacionCreateDTO
{
    public Guid HabitacionGuid { get; set; }
    public Guid? TarifaGuid { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public int NumAdultos { get; set; } = 1;
    public int NumNinos { get; set; }
    public decimal PrecioNocheAplicado { get; set; }
    public decimal SubtotalLinea { get; set; }
    public decimal ValorIvaLinea { get; set; }
    public decimal DescuentoLinea { get; set; }
    public decimal TotalLinea { get; set; }

    /// <summary>Contrato público: tipo cuando no se envía habitación concreta (reservado para futura lógica).</summary>
    public Guid? TipoHabitacionGuid { get; set; }
    public int NumHabitaciones { get; set; }
    public string? AdditionalProp1 { get; set; }
    public string? AdditionalProp2 { get; set; }
    public string? AdditionalProp3 { get; set; }
}
