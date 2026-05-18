namespace HotelLux.Auth.Business.DTOs.Roles;

public class RolCreateDTO
{
    public string NombreRol { get; set; } = null!;
    public string? DescripcionRol { get; set; }
    public string? CreadoPorUsuario { get; set; }
    public string? CreadoDesdeIp { get; set; }
}
