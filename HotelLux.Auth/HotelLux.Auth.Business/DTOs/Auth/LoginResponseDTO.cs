namespace HotelLux.Auth.Business.DTOs.Auth;

public class LoginResponseDTO
{
    public string Username { get; set; } = null!;
    public string NombreCompleto { get; set; } = null!;
    public string? Correo { get; set; }
    public bool Activo { get; set; }
    public Guid UsuarioGuid { get; set; }
    /// <summary>Cliente vinculado al usuario portal (claim cliente_guid en JWT).</summary>
    public Guid? ClienteGuid { get; set; }
    public IReadOnlyCollection<string> Roles { get; set; } = Array.Empty<string>();
    public string? Token { get; set; }
    public DateTime? ExpirationUtc { get; set; }
}
