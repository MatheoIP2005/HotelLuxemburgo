using Asp.Versioning;
using HotelLux.Accommodation.API.Models.Common;
using HotelLux.Accommodation.Business.DTOs.Habitacion;
using HotelLux.Accommodation.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelLux.Accommodation.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/internal/habitaciones")]
[Authorize]
public class HabitacionesController : ControllerBase
{
    private readonly IHabitacionService _service;

    public HabitacionesController(IHabitacionService service) => _service = service;

    public record CambiarEstadoRequest(string NuevoEstado);

    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken ct)
    {
        var result = await _service.ListarAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<HabitacionDTO>>.Ok(result, "Consulta exitosa."));
    }

    [HttpGet("{habitacionGuid:guid}")]
    public async Task<IActionResult> ObtenerPorGuid(Guid habitacionGuid, CancellationToken ct)
    {
        var result = await _service.ObtenerPorGuidAsync(habitacionGuid, ct);
        return Ok(ApiResponse<HabitacionDTO>.Ok(result));
    }

    [HttpGet("disponibilidad")]
    public async Task<IActionResult> Disponibilidad(
        [FromQuery] Guid sucursalGuid,
        [FromQuery] DateOnly fechaInicio,
        [FromQuery] DateOnly fechaFin,
        CancellationToken ct)
    {
        var result = await _service.ListarDisponiblesAsync(sucursalGuid, fechaInicio, fechaFin, ct);
        return Ok(ApiResponse<IReadOnlyList<HabitacionDTO>>.Ok(result, "Disponibilidad consultada exitosamente."));
    }

    /// <summary>Alias de spec: GET .../habitaciones/disponibles (mismos parámetros que disponibilidad).</summary>
    [HttpGet("disponibles")]
    public Task<IActionResult> Disponibles(
        [FromQuery] Guid sucursalGuid,
        [FromQuery] DateOnly fechaInicio,
        [FromQuery] DateOnly fechaFin,
        CancellationToken ct)
        => Disponibilidad(sucursalGuid, fechaInicio, fechaFin, ct);

    [HttpPost]
    [Authorize(Roles = "ADMIN,VENDEDOR")]
    public async Task<IActionResult> Crear([FromBody] HabitacionCreateDTO dto, CancellationToken ct)
    {
        dto.CreadoPorUsuario = User.FindFirst("username")?.Value ?? "api_user";
        dto.CreadoDesdeIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _service.CrearAsync(dto, ct);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<HabitacionDTO>.Created(result));
    }

    [HttpPut("{habitacionGuid:guid}")]
    [Authorize(Roles = "ADMIN,VENDEDOR")]
    public async Task<IActionResult> Actualizar(Guid habitacionGuid, [FromBody] HabitacionUpdateDTO dto, CancellationToken ct)
    {
        dto.ModificadoPorUsuario = User.FindFirst("username")?.Value ?? "api_user";
        dto.ModificadoDesdeIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _service.ActualizarAsync(habitacionGuid, dto, ct);
        return Ok(ApiResponse<HabitacionDTO>.Ok(result, "Habitación actualizada exitosamente."));
    }

    [HttpPatch("{habitacionGuid:guid}/estado")]
    [Authorize(Roles = "ADMIN,VENDEDOR")]
    public async Task<IActionResult> CambiarEstado(Guid habitacionGuid, [FromBody] CambiarEstadoRequest request, CancellationToken ct)
    {
        var usuario = User.FindFirst("username")?.Value ?? "api_user";
        await _service.CambiarEstadoAsync(habitacionGuid, request.NuevoEstado, usuario, ct);
        return NoContent();
    }

    [HttpPatch("{habitacionGuid:guid}/inhabilitar")]
    [Authorize(Roles = "ADMIN,VENDEDOR")]
    public async Task<IActionResult> Inhabilitar(Guid habitacionGuid, CancellationToken ct)
    {
        var usuario = User.FindFirst("username")?.Value ?? "api_user";
        await _service.InhabilitarAsync(habitacionGuid, usuario, ct);
        return NoContent();
    }

    [HttpDelete("{habitacionGuid:guid}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Eliminar(Guid habitacionGuid, CancellationToken ct)
    {
        var usuario = User.FindFirst("username")?.Value ?? "api_user";
        await _service.EliminarAsync(habitacionGuid, usuario, ct);
        return NoContent();
    }
}
