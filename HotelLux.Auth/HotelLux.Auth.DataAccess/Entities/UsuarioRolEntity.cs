namespace HotelLux.Auth.DataAccess.Entities;

public class UsuarioRolEntity
{
    public int IdUsuarioRol { get; set; }
    public int IdUsuario { get; set; }
    public int IdRol { get; set; }
    public string EstadoUsuarioRol { get; set; } = null!;
    public bool EsEliminado { get; set; }
    public bool Activo { get; set; }
    public DateTimeOffset FechaRegistroUtc { get; set; }
    public string CreadoPorUsuario { get; set; } = null!;
    public string? ModificadoPorUsuario { get; set; }
    public DateTimeOffset? FechaModificacionUtc { get; set; }
    public string? ModificacionIp { get; set; }

    public UsuarioAppEntity Usuario { get; set; } = null!;
    public RolEntity Rol { get; set; } = null!;
}
