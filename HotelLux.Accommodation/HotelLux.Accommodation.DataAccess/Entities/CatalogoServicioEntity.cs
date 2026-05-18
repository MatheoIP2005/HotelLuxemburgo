namespace HotelLux.Accommodation.DataAccess.Entities;

public class CatalogoServicioEntity
{
    public int IdCatalogo { get; set; }
    public Guid CatalogoGuid { get; set; }
    public int? IdSucursal { get; set; }
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
    public bool EsEliminado { get; set; }
    public DateTimeOffset? FechaInhabilitacionUtc { get; set; }
    public string? MotivoInhabilitacion { get; set; }
    public DateTimeOffset FechaRegistroUtc { get; set; }
    public string CreadoPorUsuario { get; set; } = null!;
    public string? ModificadoPorUsuario { get; set; }
    public DateTimeOffset? FechaModificacionUtc { get; set; }
    public string? ModificacionIp { get; set; }
    public string ServicioOrigen { get; set; } = null!;

    public SucursalEntity? Sucursal { get; set; }
    public ICollection<TipoHabitacionCatalogoEntity> TipoHabitacionCatalogos { get; set; } = new List<TipoHabitacionCatalogoEntity>();
}
