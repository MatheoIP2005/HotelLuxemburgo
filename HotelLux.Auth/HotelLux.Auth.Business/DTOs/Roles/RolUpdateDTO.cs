namespace HotelLux.Auth.Business.DTOs.Roles;

public class RolUpdateDTO
{
    public string NombreRol { get; set; } = null!;
    public string? DescripcionRol { get; set; }
    public string EstadoRol { get; set; } = "ACT";
    public string? ModificadoPorUsuario { get; set; }
    public string? ModificadoDesdeIp { get; set; }
}
