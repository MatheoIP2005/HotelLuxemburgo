namespace HotelLux.Auth.Business.DTOs.Usuarios;

public class UsuarioDTO
{
    public int IdUsuario { get; set; }
    public Guid UsuarioGuid { get; set; }
    public Guid? ClienteGuid { get; set; }
    public string Username { get; set; } = null!;
    public string Correo { get; set; } = null!;
    public string Nombres { get; set; } = null!;
    public string? Apellidos { get; set; }
    public string EstadoUsuario { get; set; } = null!;
    public bool Activo { get; set; }
    public DateTimeOffset FechaRegistroUtc { get; set; }
    public string CreadoPorUsuario { get; set; } = null!;
    public IReadOnlyList<string> Roles { get; set; } = new List<string>();
}
