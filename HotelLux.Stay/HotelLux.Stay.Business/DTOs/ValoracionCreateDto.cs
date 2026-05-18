using System.ComponentModel.DataAnnotations;

namespace HotelLux.Stay.Business.DTOs;

public class ValoracionCreateDto
{
    [Required]
    public Guid EstadiaGuid { get; set; }

    [Required, Range(1.0, 5.0)]
    public decimal PuntuacionGeneral { get; set; }

    [Required, Range(1.0, 5.0)]
    public decimal PuntuacionLimpieza { get; set; }

    [Required, Range(1.0, 5.0)]
    public decimal PuntuacionConfort { get; set; }

    [Required, Range(1.0, 5.0)]
    public decimal PuntuacionUbicacion { get; set; }

    [Required, Range(1.0, 5.0)]
    public decimal PuntuacionInstalaciones { get; set; }

    [Required, Range(1.0, 5.0)]
    public decimal PuntuacionPersonal { get; set; }

    [Required, Range(1.0, 5.0)]
    public decimal PuntuacionCalidadPrecio { get; set; }

    [Required, MaxLength(2000)]
    public string ComentarioPositivo { get; set; } = null!;

    [Required, MaxLength(2000)]
    public string ComentarioNegativo { get; set; } = null!;

    [Required, MaxLength(50)]
    public string TipoViaje { get; set; } = null!;   // NEGOCIO | FAMILIA | PAREJA | SOLITARIO | AMIGOS

    [MaxLength(150)]
    public string? NombreVisibleCliente { get; set; }

    public string? CreadoPorUsuario { get; set; }
}

public class ValoracionDto
{
    public Guid   ValoracionGuid       { get; set; }
    public Guid   EstadiaGuid          { get; set; }
    public Guid   SucursalGuid         { get; set; }
    public Guid   ClienteGuid          { get; set; }
    public decimal PuntuacionGeneral   { get; set; }
    public string ComentarioPositivo   { get; set; } = null!;
    public string ComentarioNegativo   { get; set; } = null!;
    public string TipoViaje            { get; set; } = null!;
    public DateTimeOffset FechaPublicacionUtc { get; set; }
    public string? RespuestaHotel      { get; set; }
    public string? NombreVisibleCliente { get; set; }
}

public class ValoracionResponderDto
{
    [MaxLength(2000)]
    public string? Respuesta { get; set; }

    [MaxLength(2000)]
    public string? RespuestaHotel { get; set; }
}
