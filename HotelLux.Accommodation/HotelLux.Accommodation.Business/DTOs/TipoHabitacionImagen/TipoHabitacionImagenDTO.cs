namespace HotelLux.Accommodation.Business.DTOs.TipoHabitacionImagen;

public class TipoHabitacionImagenDTO
{
    public int IdTipoHabitacionImagen { get; set; }
    public string UrlImagen { get; set; } = null!;
    public string? DescripcionImagen { get; set; }
    public int OrdenVisualizacion { get; set; }
    public bool EsPrincipal { get; set; }
    public DateTimeOffset FechaRegistroUtc { get; set; }
}
