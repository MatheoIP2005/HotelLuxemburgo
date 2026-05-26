using HotelLux.Auth.Business.DTOs.Auth;
using HotelLux.Auth.Business.Exceptions;
using HotelLux.Auth.Business.Interfaces;
using HotelLux.Auth.DataManagement.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;

namespace HotelLux.Auth.Business.Services;

public class AuthService : IAuthService
{
    private readonly IServiceProvider _serviceProvider;
    private static readonly ConcurrentDictionary<Guid, HashSet<string>> RevokedRefreshJtiByUser = new();
    private static readonly ConcurrentDictionary<Guid, int> TokenVersionByUser = new();
    private static readonly ConcurrentDictionary<string, Guid> RefreshTokenIndex = new();

    public AuthService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<LoginResponseDTO> LoginAsync(LoginRequestDTO dto, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var usuarioDataService = scope.ServiceProvider.GetRequiredService<IUsuarioDataService>();

        if (string.IsNullOrWhiteSpace(dto.Username))
            throw new ValidationException("El nombre de usuario es obligatorio.");

        if (string.IsNullOrWhiteSpace(dto.Password))
            throw new ValidationException("La contraseña es obligatoria.");

        var loginData = await usuarioDataService.ObtenerParaLoginAsync(dto.Username, cancellationToken);

        if (loginData is null || !loginData.Activo || loginData.EsEliminado || loginData.EstadoUsuario != "ACT")
            throw new UnauthorizedBusinessException("Credenciales inválidas");

        if (!VerificarPassword(dto.Password, loginData.PasswordHash, loginData.PasswordSalt))
            throw new UnauthorizedBusinessException("Credenciales inválidas");

        return new LoginResponseDTO
        {
            UsuarioId = loginData.UsuarioId,
            Username = loginData.Username,
            NombreCompleto = $"{loginData.Nombres} {loginData.Apellidos}".Trim(),
            Correo = loginData.Correo,
            Activo = loginData.Activo,
            UsuarioGuid = loginData.UsuarioGuid,
            ClienteGuid = loginData.ClienteGuid,
            Roles = loginData.Roles
        };
    }

    public async Task<LoginResponseDTO> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var usuarioDataService = scope.ServiceProvider.GetRequiredService<IUsuarioDataService>();

        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new UnauthorizedBusinessException("Refresh token inválido.");

        string username;
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwt = tokenHandler.ReadJwtToken(refreshToken);
            username = jwt.Claims.FirstOrDefault(c => c.Type == "username")?.Value
                ?? jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value
                ?? string.Empty;
        }
        catch
        {
            throw new UnauthorizedBusinessException("Refresh token inválido.");
        }

        if (string.IsNullOrWhiteSpace(username))
            throw new UnauthorizedBusinessException("Refresh token inválido.");

        var loginData = await usuarioDataService.ObtenerParaLoginAsync(username, cancellationToken);
        if (loginData is null || !loginData.Activo || loginData.EsEliminado || loginData.EstadoUsuario != "ACT")
            throw new UnauthorizedBusinessException("Usuario inválido o inactivo.");

        return new LoginResponseDTO
        {
            UsuarioId = loginData.UsuarioId,
            Username = loginData.Username,
            NombreCompleto = $"{loginData.Nombres} {loginData.Apellidos}".Trim(),
            Correo = loginData.Correo,
            Activo = loginData.Activo,
            UsuarioGuid = loginData.UsuarioGuid,
            ClienteGuid = loginData.ClienteGuid,
            Roles = loginData.Roles
        };
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return;

        if (RefreshTokenIndex.TryGetValue(refreshToken, out var userGuid))
        {
            RefreshTokenIndex.TryRemove(refreshToken, out _);
            var jti = ExtraerJti(refreshToken);
            if (!string.IsNullOrWhiteSpace(jti))
                RevocarRefreshToken(userGuid, jti);
        }
        await Task.CompletedTask;
    }

    public async Task CambiarPasswordAsync(string username, string passwordActual, string passwordNuevo, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var usuarioDataService = scope.ServiceProvider.GetRequiredService<IUsuarioDataService>();

        var loginData = await usuarioDataService.ObtenerParaLoginAsync(username, cancellationToken);
        if (loginData is null)
            throw new NotFoundException($"No se encontró el usuario '{username}'.");

        if (!VerificarPassword(passwordActual, loginData.PasswordHash, loginData.PasswordSalt))
            throw new ValidationException("La contraseña actual es incorrecta.");

        if (string.IsNullOrWhiteSpace(passwordNuevo) || passwordNuevo.Length < 6)
            throw new ValidationException("La nueva contraseña debe tener al menos 6 caracteres.");

        var usuario = await usuarioDataService.ObtenerPorUsernameAsync(username, cancellationToken);
        if (usuario is null)
            throw new NotFoundException("Usuario", username);

        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(passwordNuevo);
        usuario.PasswordSalt = string.Empty;
        usuario.ModificadoPorUsuario = username;
        usuario.FechaModificacionUtc = DateTimeOffset.UtcNow;

        await usuarioDataService.ActualizarAsync(usuario, cancellationToken);
        RevocarTodosLosRefreshTokens(usuario.UsuarioGuid);
    }

    public void RegistrarRefreshToken(Guid usuarioGuid, string refreshToken, string jti)
    {
        RefreshTokenIndex[refreshToken] = usuarioGuid;
        RevokedRefreshJtiByUser.TryAdd(usuarioGuid, new HashSet<string>(StringComparer.Ordinal));
        TokenVersionByUser.TryAdd(usuarioGuid, 1);
    }

    public bool EsRefreshTokenRevocado(Guid usuarioGuid, string jti)
    {
        return RevokedRefreshJtiByUser.TryGetValue(usuarioGuid, out var set) && set.Contains(jti);
    }

    public void RevocarRefreshToken(Guid usuarioGuid, string jti)
    {
        var set = RevokedRefreshJtiByUser.GetOrAdd(usuarioGuid, _ => new HashSet<string>(StringComparer.Ordinal));
        lock (set)
        {
            set.Add(jti);
        }
    }

    public void RevocarTodosLosRefreshTokens(Guid usuarioGuid)
    {
        TokenVersionByUser.AddOrUpdate(usuarioGuid, 2, (_, v) => v + 1);
        RevokedRefreshJtiByUser.TryRemove(usuarioGuid, out _);
    }

    public int ObtenerVersionToken(Guid usuarioGuid)
    {
        return TokenVersionByUser.GetOrAdd(usuarioGuid, 1);
    }

    private static bool VerificarPassword(string inputPassword, string storedHash, string storedSalt)
    {
        if (storedHash.StartsWith("$2", StringComparison.Ordinal))
            return BCrypt.Net.BCrypt.Verify(inputPassword, storedHash);

        var legacyHash = Convert.ToBase64String(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(inputPassword + storedSalt)));
        return legacyHash == storedHash;
    }

    private static string ExtraerJti(string token)
    {
        try
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            return jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }
}
