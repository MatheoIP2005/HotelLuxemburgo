namespace HotelLux.Accommodation.Business.DTOs.Habitacion;

public class HabitacionCreateDTO
{
    public Guid SucursalGuid { get; set; }
    public Guid TipoHabitacionGuid { get; set; }
    public string NumeroHabitacion { get; set; } = null!;
    public int? Piso { get; set; }
    public int CapacidadHabitacion { get; set; }
    public decimal PrecioBase { get; set; }
    public string? DescripcionHabitacion { get; set; }
    public string? CreadoPorUsuario { get; set; }
    public string? CreadoDesdeIp { get; set; }
}
