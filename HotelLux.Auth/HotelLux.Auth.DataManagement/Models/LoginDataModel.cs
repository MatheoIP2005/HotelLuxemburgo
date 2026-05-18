namespace HotelLux.Auth.DataManagement.Models;

public class LoginDataModel
{
    public string Username { get; set; } = null!;
    public string Nombres { get; set; } = null!;
    public string? Apellidos { get; set; }
    public string? Correo { get; set; }
    public string PasswordHash { get; set; } = null!;
    public string PasswordSalt { get; set; } = null!;
    public string EstadoUsuario { get; set; } = null!;
    public bool Activo { get; set; }
    public bool EsEliminado { get; set; }
    public Guid UsuarioGuid { get; set; }
    public Guid? ClienteGuid { get; set; }
    public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();
}
