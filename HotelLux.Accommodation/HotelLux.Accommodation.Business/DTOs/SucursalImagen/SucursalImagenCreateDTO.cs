namespace HotelLux.Accommodation.Business.DTOs.SucursalImagen;

public class SucursalImagenCreateDTO
{
    public string UrlImagen { get; set; } = null!;
    public string? DescripcionImagen { get; set; }
    public int OrdenVisualizacion { get; set; } = 1;
    public bool EsPrincipal { get; set; }
    public string? CreadoPorUsuario { get; set; }
}
