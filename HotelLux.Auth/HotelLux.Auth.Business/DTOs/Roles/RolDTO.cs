namespace HotelLux.Auth.Business.DTOs.Roles;

public class RolDTO
{
    public int IdRol { get; set; }
    public Guid RolGuid { get; set; }
    public string NombreRol { get; set; } = null!;
    public string? DescripcionRol { get; set; }
    public string EstadoRol { get; set; } = null!;
    public bool Activo { get; set; }
    public DateTimeOffset FechaRegistroUtc { get; set; }
    public string CreadoPorUsuario { get; set; } = null!;
}
