namespace HotelLux.Accommodation.Business.DTOs.SucursalImagen;

public class SucursalImagenDTO
{
    public int IdSucursalImagen { get; set; }
    public Guid SucursalImagenGuid { get; set; }
    public string UrlImagen { get; set; } = null!;
    public string? DescripcionImagen { get; set; }
    public int OrdenVisualizacion { get; set; }
    public bool EsPrincipal { get; set; }
    public DateTimeOffset FechaRegistroUtc { get; set; }
}
