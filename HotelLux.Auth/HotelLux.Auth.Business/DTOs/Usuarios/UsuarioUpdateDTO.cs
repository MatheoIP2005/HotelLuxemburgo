namespace HotelLux.Auth.Business.DTOs.Usuarios;

public class UsuarioUpdateDTO
{
    public string Username { get; set; } = null!;
    public string Correo { get; set; } = null!;
    public string Nombres { get; set; } = null!;
    public string? Apellidos { get; set; }
    public string EstadoUsuario { get; set; } = "ACT";
    public string? MotivoInhabilitacion { get; set; }
    public string? ModificadoPorUsuario { get; set; }
    public string? ModificadoDesdeIp { get; set; }
}
