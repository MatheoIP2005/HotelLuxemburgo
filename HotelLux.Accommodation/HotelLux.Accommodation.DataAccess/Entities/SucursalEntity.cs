namespace HotelLux.Accommodation.DataAccess.Entities;

public class SucursalEntity
{
    public int IdSucursal { get; set; }
    public Guid SucursalGuid { get; set; }
    public string CodigoSucursal { get; set; } = null!;
    public string NombreSucursal { get; set; } = null!;
    public string? DescripcionSucursal { get; set; }
    public string? DescripcionCorta { get; set; }
    public string TipoAlojamiento { get; set; } = null!;
    public int? Estrellas { get; set; }
    public string? CategoriaViaje { get; set; }
    public string Pais { get; set; } = null!;
    public string? Provincia { get; set; }
    public string Ciudad { get; set; } = null!;
    public string Ubicacion { get; set; } = null!;
    public string Direccion { get; set; } = null!;
    public string? CodigoPostal { get; set; }
    public string Telefono { get; set; } = null!;
    public string Correo { get; set; } = null!;
    public decimal? Latitud { get; set; }
    public decimal? Longitud { get; set; }
    public string? HoraCheckin { get; set; }
    public string? HoraCheckout { get; set; }
    public bool CheckinAnticipado { get; set; }
    public bool CheckoutTardio { get; set; }
    public bool AceptaNinos { get; set; }
    public int? EdadMinimaHuesped { get; set; }
    public bool PermiteMascotas { get; set; }
    public bool SePermiteFumar { get; set; }
    public string EstadoSucursal { get; set; } = null!;
    public bool EsEliminado { get; set; }
    public DateTimeOffset? FechaInhabilitacionUtc { get; set; }
    public string? MotivoInhabilitacion { get; set; }
    public DateTimeOffset FechaRegistroUtc { get; set; }
    public string CreadoPorUsuario { get; set; } = null!;
    public string? ModificadoPorUsuario { get; set; }
    public DateTimeOffset? FechaModificacionUtc { get; set; }
    public string? ModificacionIp { get; set; }
    public string ServicioOrigen { get; set; } = null!;

    public ICollection<SucursalImagenEntity> Imagenes { get; set; } = new List<SucursalImagenEntity>();
    public ICollection<HabitacionEntity> Habitaciones { get; set; } = new List<HabitacionEntity>();
    public ICollection<TarifaEntity> Tarifas { get; set; } = new List<TarifaEntity>();
    public ICollection<CatalogoServicioEntity> CatalogoServicios { get; set; } = new List<CatalogoServicioEntity>();
}
