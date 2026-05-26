namespace HotelLux.Auth.API.Models.Contracts;

public class LoginSuccessResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = null!;
    public LoginSuccessData Data { get; set; } = null!;
    public object? Errors { get; set; }

    public static LoginSuccessResponse Ok(LoginSuccessData data)
    {
        return new LoginSuccessResponse
        {
            Success = true,
            Message = "Autenticación exitosa",
            Data = data,
            Errors = null
        };
    }
}

public class LoginSuccessData
{
    public string Token { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public DateTime Expiration { get; set; }
    public int UsuarioId { get; set; }
    public Guid UsuarioGuid { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public IReadOnlyCollection<string> Roles { get; set; } = Array.Empty<string>();
}
