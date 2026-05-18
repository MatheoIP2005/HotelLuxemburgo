namespace HotelLux.Accommodation.DataManagement.Models;

public class SucursalImagenDataModel
{
    public int IdSucursalImagen { get; set; }
    public Guid SucursalImagenGuid { get; set; }
    public int IdSucursal { get; set; }
    public string UrlImagen { get; set; } = null!;
    public string? DescripcionImagen { get; set; }
    public int OrdenVisualizacion { get; set; }
    public bool EsPrincipal { get; set; }
    public DateTimeOffset FechaRegistroUtc { get; set; }
    public string CreadoPorUsuario { get; set; } = null!;
}
