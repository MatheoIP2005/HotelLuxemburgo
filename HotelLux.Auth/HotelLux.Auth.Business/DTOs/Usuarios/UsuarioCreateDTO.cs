namespace HotelLux.Auth.Business.DTOs.Usuarios;

public class UsuarioCreateDTO
{
    public Guid? ClienteGuid { get; set; }
    public string Username { get; set; } = null!;
    public string Correo { get; set; } = null!;
    public string Nombres { get; set; } = null!;
    public string? Apellidos { get; set; }
    public string Password { get; set; } = null!;
    public string? CreadoPorUsuario { get; set; }
    public string? CreadoDesdeIp { get; set; }
}
