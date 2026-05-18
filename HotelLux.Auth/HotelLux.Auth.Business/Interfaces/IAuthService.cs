using HotelLux.Auth.Business.DTOs.Auth;

namespace HotelLux.Auth.Business.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDTO> LoginAsync(LoginRequestDTO dto, CancellationToken cancellationToken = default);
    Task<LoginResponseDTO> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task CambiarPasswordAsync(string username, string passwordActual, string passwordNuevo, CancellationToken cancellationToken = default);
    void RegistrarRefreshToken(Guid usuarioGuid, string refreshToken, string jti);
    bool EsRefreshTokenRevocado(Guid usuarioGuid, string jti);
    void RevocarRefreshToken(Guid usuarioGuid, string jti);
    void RevocarTodosLosRefreshTokens(Guid usuarioGuid);
    int ObtenerVersionToken(Guid usuarioGuid);
}
