namespace HotelLux.Auth.API.Models.Contracts;

public class CambiarPasswordRequest
{
    public string passwordActual { get; set; } = string.Empty;
    public string passwordNuevo { get; set; } = string.Empty;
    /// <summary>Alias spec OpenAPI (<c>nuevaPassword</c>).</summary>
    public string? nuevaPassword { get; set; }
}
