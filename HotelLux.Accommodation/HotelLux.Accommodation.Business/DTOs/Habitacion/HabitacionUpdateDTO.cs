namespace HotelLux.Accommodation.Business.DTOs.Habitacion;

public class HabitacionUpdateDTO
{
    public string NumeroHabitacion { get; set; } = null!;
    public int? Piso { get; set; }
    public int CapacidadHabitacion { get; set; }
    public decimal PrecioBase { get; set; }
    public string? DescripcionHabitacion { get; set; }
    public string EstadoHabitacion { get; set; } = null!;
    public string? ModificadoPorUsuario { get; set; }
    public string? ModificadoDesdeIp { get; set; }
}
