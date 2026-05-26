using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Asp.Versioning;
using HotelLux.Auth.API.Models.Common;
using HotelLux.Auth.API.Models.Contracts;
using HotelLux.Auth.API.Models.Settings;
using HotelLux.Auth.Business.DTOs.Auth;
using HotelLux.Auth.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HotelLux.Auth.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly JwtSettings _jwtSettings;
    private readonly IAuditEmitter _auditEmitter;

    public AuthController(IAuthService authService, IOptions<JwtSettings> jwtOptions, IAuditEmitter auditEmitter)
    {
        _authService = authService;
        _jwtSettings = jwtOptions.Value;
        _auditEmitter = auditEmitter;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginSuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDTO request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);

        var now = DateTime.UtcNow;
        var accessJti = Guid.NewGuid().ToString("N");
        var refreshJti = Guid.NewGuid().ToString("N");
        var accessExpiration = now.AddSeconds(_jwtSettings.JwtExpiresIn);
        var refreshExpiration = now.AddSeconds(_jwtSettings.JwtRefreshExpiresIn);
        var tokenVersion = _authService.ObtenerVersionToken(result.UsuarioGuid);

        var accessToken = BuildToken(
            _jwtSettings.JwtSecret,
            _jwtSettings.Issuer,
            _jwtSettings.Audience,
            result,
            accessJti,
            accessExpiration,
            tokenVersion);

        var refreshToken = BuildToken(
            _jwtSettings.JwtRefreshSecret,
            _jwtSettings.Issuer,
            _jwtSettings.Audience,
            result,
            refreshJti,
            refreshExpiration,
            tokenVersion);

        _authService.RegistrarRefreshToken(result.UsuarioGuid, refreshToken, refreshJti);

        _ = _auditEmitter.EmitAsync(
            "seguridad.usuario_app",
            "LOGIN",
            result.UsuarioGuid.ToString(),
            result.UsuarioGuid.ToString(),
            $"{{\"username\":\"{result.Username}\"}}",
            CancellationToken.None);

        return Ok(LoginSuccessResponse.Ok(new LoginSuccessData
        {
            Token = accessToken,
            RefreshToken = refreshToken,
            Expiration = accessExpiration,
            UsuarioId = result.UsuarioId,
            UsuarioGuid = result.UsuarioGuid,
            Username = result.Username,
            Email = result.Correo ?? string.Empty,
            Roles = result.Roles
        }));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RefreshTokenAsync(request.refreshToken, cancellationToken);
        ClaimsPrincipal refreshValidation;
        Guid userGuid;
        string jti;
        int tokenVersion;
        try
        {
            var handler = new JwtSecurityTokenHandler();
            refreshValidation = handler.ValidateToken(
                request.refreshToken,
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.JwtRefreshSecret)),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                },
                out _);

            var usuarioGuidClaim = refreshValidation.FindFirst("usuario_guid")?.Value
                ?? throw new InvalidOperationException();
            userGuid = Guid.Parse(usuarioGuidClaim);
            jti = refreshValidation.FindFirst(JwtRegisteredClaimNames.Jti)?.Value ?? string.Empty;
            tokenVersion = int.Parse(refreshValidation.FindFirst("token_version")?.Value ?? "1");
        }
        catch
        {
            return Unauthorized(ApiErrorResponse.Fail(StatusCodes.Status401Unauthorized, "Credenciales inválidas"));
        }

        if (_authService.EsRefreshTokenRevocado(userGuid, jti) || tokenVersion != _authService.ObtenerVersionToken(userGuid))
            return Unauthorized(ApiErrorResponse.Fail(StatusCodes.Status401Unauthorized, "Credenciales inválidas"));

        var accessJti = Guid.NewGuid().ToString("N");
        var accessExpiration = DateTime.UtcNow.AddSeconds(_jwtSettings.JwtExpiresIn);
        var accessToken = BuildToken(_jwtSettings.JwtSecret, _jwtSettings.Issuer, _jwtSettings.Audience, result, accessJti, accessExpiration, tokenVersion);

        return Ok(new
        {
            access_token = accessToken,
            expires_in = _jwtSettings.JwtExpiresIn,
            cliente_guid = result.ClienteGuid
        });
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken cancellationToken)
    {
        await _authService.LogoutAsync(request.refreshToken, cancellationToken);

        _ = _auditEmitter.EmitAsync(
            "seguridad.usuario_app",
            "LOGOUT",
            string.Empty,
            User.FindFirst("usuario_guid")?.Value ?? string.Empty,
            "{}",
            CancellationToken.None);

        return NoContent();
    }

    [HttpPost("cambiar-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CambiarPassword([FromBody] CambiarPasswordRequest request, CancellationToken cancellationToken)
    {
        var username = User.FindFirst("username")?.Value ?? string.Empty;
        var nueva = string.IsNullOrWhiteSpace(request.nuevaPassword) ? request.passwordNuevo : request.nuevaPassword!;
        await _authService.CambiarPasswordAsync(username, request.passwordActual, nueva, cancellationToken);
        return NoContent();
    }

    private static string BuildToken(
        string secret,
        string issuer,
        string audience,
        LoginResponseDTO user,
        string jti,
        DateTime expirationUtc,
        int tokenVersion)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UsuarioGuid.ToString()),
            new("username", user.Username),
            new("correo", user.Correo ?? string.Empty),
            new("usuario_guid", user.UsuarioGuid.ToString()),
            new(JwtRegisteredClaimNames.Jti, jti),
            new("token_version", tokenVersion.ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };
        claims.AddRange(user.Roles.Select(r => new Claim("roles", r)));
        claims.AddRange(user.Roles.Select(r => new Claim(ClaimTypes.Role, r)));
        if (user.ClienteGuid.HasValue && user.ClienteGuid.Value != Guid.Empty)
            claims.Add(new Claim("cliente_guid", user.ClienteGuid.Value.ToString()));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(issuer, audience, claims, expires: expirationUtc, signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
