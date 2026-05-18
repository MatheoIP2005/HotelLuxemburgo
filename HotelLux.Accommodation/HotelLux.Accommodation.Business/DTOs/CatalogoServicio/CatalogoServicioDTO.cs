namespace HotelLux.Accommodation.Business.DTOs.CatalogoServicio;

public class CatalogoServicioDTO
{
    public Guid CatalogoGuid { get; set; }
    public Guid? SucursalGuid { get; set; }
    public string CodigoCatalogo { get; set; } = null!;
    public string NombreCatalogo { get; set; } = null!;
    public string TipoCatalogo { get; set; } = null!;
    public string CategoriaCatalogo { get; set; } = null!;
    public string? DescripcionCatalogo { get; set; }
    public decimal PrecioBase { get; set; }
    public bool AplicaIva { get; set; }
    public bool Disponible24h { get; set; }
    public TimeOnly? HoraInicio { get; set; }
    public TimeOnly? HoraFin { get; set; }
    public string? IconoUrl { get; set; }
    public string EstadoCatalogo { get; set; } = null!;
    public DateTimeOffset FechaRegistroUtc { get; set; }
    public string CreadoPorUsuario { get; set; } = null!;
}
