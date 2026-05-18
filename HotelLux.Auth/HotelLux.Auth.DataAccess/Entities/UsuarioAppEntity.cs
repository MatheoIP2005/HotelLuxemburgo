namespace HotelLux.Auth.DataAccess.Entities;

public class UsuarioAppEntity
{
    public int IdUsuario { get; set; }
    public Guid UsuarioGuid { get; set; }
    public Guid? ClienteGuid { get; set; }
    public string Username { get; set; } = null!;
    public string Correo { get; set; } = null!;
    public string Nombres { get; set; } = null!;
    public string? Apellidos { get; set; }
    public string PasswordHash { get; set; } = null!;
    public string PasswordSalt { get; set; } = null!;
    public string EstadoUsuario { get; set; } = null!;
    public bool EsEliminado { get; set; }
    public bool Activo { get; set; }
    public DateTimeOffset? FechaInhabilitacionUtc { get; set; }
    public string? MotivoInhabilitacion { get; set; }
    public DateTimeOffset FechaRegistroUtc { get; set; }
    public string CreadoPorUsuario { get; set; } = null!;
    public string? ModificadoPorUsuario { get; set; }
    public DateTimeOffset? FechaModificacionUtc { get; set; }
    public string? ModificacionIp { get; set; }

    public ICollection<UsuarioRolEntity> UsuarioRoles { get; set; } = new List<UsuarioRolEntity>();
}
