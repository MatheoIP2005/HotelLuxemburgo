namespace HotelLux.Auth.API.Models.Settings;

public class JwtSettings
{
    public string JwtSecret { get; set; } = string.Empty;
    public string JwtRefreshSecret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int JwtExpiresIn { get; set; }
    public int JwtRefreshExpiresIn { get; set; }
}
