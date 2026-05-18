namespace HotelLux.Auth.DataAccess.Entities;

public class RolEntity
{
    public int IdRol { get; set; }
    public Guid RolGuid { get; set; }
    public string NombreRol { get; set; } = null!;
    public string? DescripcionRol { get; set; }
    public string EstadoRol { get; set; } = null!;
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
