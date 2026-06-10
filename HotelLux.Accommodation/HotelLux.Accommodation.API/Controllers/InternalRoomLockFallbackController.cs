using System.Security.Cryptography;
using System.Text;
using Asp.Versioning;
using HotelLux.Accommodation.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelLux.Accommodation.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/internal/render-fallback/habitaciones")]
[AllowAnonymous]
public sealed class InternalRoomLockFallbackController : ControllerBase
{
    private const string ServiceKeyHeader = "X-Internal-Service-Key";

    private readonly IHabitacionService _habitaciones;
    private readonly IConfiguration _configuration;

    public InternalRoomLockFallbackController(
        IHabitacionService habitaciones,
        IConfiguration configuration)
    {
        _habitaciones = habitaciones;
        _configuration = configuration;
    }

    public sealed record CambiarEstadoRequest(string NuevoEstado);

    public sealed record ConfirmarBloqueoRequest(Guid ReservaGuid);

    [HttpPost("{habitacionGuid:guid}/confirmar-bloqueo")]
    public async Task<IActionResult> ConfirmarBloqueo(
        Guid habitacionGuid,
        [FromBody] ConfirmarBloqueoRequest request,
        CancellationToken ct)
    {
        if (!IsAuthorized())
            return Unauthorized(new
            {
                status = 401,
                error = "No autorizado",
                details = new[] { "Clave interna requerida o invalida." },
                timestamp = DateTime.UtcNow
            });

        var result = await _habitaciones.ConfirmarBloqueoReservaAsync(
            habitacionGuid,
            request.ReservaGuid,
            ct);

        if (!result.Success)
        {
            return Conflict(new
            {
                status = 409,
                error = "Conflicto",
                details = new[] { result.Message },
                timestamp = DateTime.UtcNow
            });
        }

        return Ok(new
        {
            success = true,
            mensaje = result.Message,
            timestamp = DateTime.UtcNow
        });
    }

    [HttpPost("{habitacionGuid:guid}/liberar-bloqueo")]
    public async Task<IActionResult> LiberarBloqueo(
        Guid habitacionGuid,
        [FromBody] ConfirmarBloqueoRequest request,
        CancellationToken ct)
    {
        if (!IsAuthorized())
            return Unauthorized(new
            {
                status = 401,
                error = "No autorizado",
                details = new[] { "Clave interna requerida o invalida." },
                timestamp = DateTime.UtcNow
            });

        var result = await _habitaciones.LiberarBloqueoReservaAsync(
            habitacionGuid,
            request.ReservaGuid,
            ct);

        return Ok(new
        {
            success = result.Success,
            mensaje = result.Message,
            timestamp = DateTime.UtcNow
        });
    }

    [HttpPatch("{habitacionGuid:guid}/estado")]
    public async Task<IActionResult> CambiarEstado(
        Guid habitacionGuid,
        [FromBody] CambiarEstadoRequest request,
        CancellationToken ct)
    {
        if (!IsAuthorized())
            return Unauthorized(new
            {
                status = 401,
                error = "No autorizado",
                details = new[] { "Clave interna requerida o invalida." },
                timestamp = DateTime.UtcNow
            });

        await _habitaciones.CambiarEstadoAsync(
            habitacionGuid,
            request.NuevoEstado,
            "reservation-rest-fallback",
            ct);

        return NoContent();
    }

    private bool IsAuthorized()
    {
        var expected = _configuration["InternalService:FallbackKey"];
        if (string.IsNullOrWhiteSpace(expected))
            return false;

        var actual = Request.Headers[ServiceKeyHeader].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(actual))
            return false;

        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        var actualBytes = Encoding.UTF8.GetBytes(actual);

        return expectedBytes.Length == actualBytes.Length &&
               CryptographicOperations.FixedTimeEquals(actualBytes, expectedBytes);
    }
}
