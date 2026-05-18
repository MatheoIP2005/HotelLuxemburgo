namespace HotelLux.Accommodation.DataAccess.Entities;

public class TipoHabitacionImagenEntity
{
    public int IdTipoHabitacionImagen { get; set; }
    public int IdTipoHabitacion { get; set; }
    public string UrlImagen { get; set; } = null!;
    public string? DescripcionImagen { get; set; }
    public int OrdenVisualizacion { get; set; }
    public bool EsPrincipal { get; set; }
    public DateTimeOffset FechaRegistroUtc { get; set; }
    public string CreadoPorUsuario { get; set; } = null!;

    public TipoHabitacionEntity TipoHabitacion { get; set; } = null!;
}
