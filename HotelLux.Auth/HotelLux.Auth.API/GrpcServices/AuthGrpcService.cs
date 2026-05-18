using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Grpc.Core;
using HotelLux.Auth.API.Models.Settings;
using HotelLux.Auth.DataManagement.Interfaces;
using HotelLux.Protos.Auth;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HotelLux.Auth.API.GrpcServices;

public class AuthGrpcService : AuthService.AuthServiceBase
{
    private readonly JwtSettings _jwt;
    private readonly IServiceScopeFactory _scopeFactory;

    public AuthGrpcService(IOptions<JwtSettings> jwtOptions, IServiceScopeFactory scopeFactory)
    {
        _jwt = jwtOptions.Value;
        _scopeFactory = scopeFactory;
    }

    public override Task<ValidateTokenResponse> ValidateToken(ValidateTokenRequest request, ServerCallContext context)
    {
        var response = new ValidateTokenResponse();
        if (string.IsNullOrWhiteSpace(request.Token))
        {
            response.Valid = false;
            response.MensajeError = "Token vacío.";
            return Task.FromResult(response);
        }

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(
                request.Token,
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.JwtSecret)),
                    ValidateIssuer = true,
                    ValidIssuer = _jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwt.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                },
                out var validatedToken);

            var jwt = (JwtSecurityToken)validatedToken;
            var usuarioGuid = principal.FindFirst("usuario_guid")?.Value ?? string.Empty;
            var username = principal.FindFirst("username")?.Value ?? string.Empty;
            var clienteGuid = principal.FindFirst("cliente_guid")?.Value ?? string.Empty;

            var roles = principal.FindAll("roles").Select(c => c.Value)
                .Concat(principal.FindAll(ClaimTypes.Role).Select(c => c.Value))
                .Distinct(StringComparer.Ordinal)
                .ToList();

            response.Valid = true;
            response.UserGuid = usuarioGuid;
            response.Username = username;
            response.ClienteGuid = clienteGuid;
            response.ExpiraEnUnix = new DateTimeOffset(jwt.ValidTo.ToUniversalTime()).ToUnixTimeSeconds();
            response.Roles.AddRange(roles);
            response.MensajeError = string.Empty;
        }
        catch (Exception ex)
        {
            response.Valid = false;
            response.MensajeError = ex.Message;
        }

        return Task.FromResult(response);
    }

    public override async Task<GetUserRolesResponse> GetUserRoles(GetUserRolesRequest request, ServerCallContext context)
    {
        var response = new GetUserRolesResponse();
        if (!Guid.TryParse(request.UserGuid, out var guid))
            return response;

        using var scope = _scopeFactory.CreateScope();
        var usuarioDataService = scope.ServiceProvider.GetRequiredService<IUsuarioDataService>();
        var user = await usuarioDataService.ObtenerPorGuidAsync(guid, context.CancellationToken);
        if (user?.Roles is { Count: > 0 })
            response.Roles.AddRange(user.Roles);
        return response;
    }
}
