namespace HotelLux.Auth.Business.DTOs.Auth;

public class CambiarPasswordDTO
{
    public string PasswordActual { get; set; } = null!;
    public string PasswordNueva { get; set; } = null!;
}
