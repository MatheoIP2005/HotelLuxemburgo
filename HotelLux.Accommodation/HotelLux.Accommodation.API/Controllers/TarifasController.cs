using Asp.Versioning;
using HotelLux.Accommodation.API.Models.Common;
using HotelLux.Accommodation.Business.DTOs.Tarifa;
using HotelLux.Accommodation.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelLux.Accommodation.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/internal/tarifas")]
[Authorize]
public class TarifasController : ControllerBase
{
    private readonly ITarifaService _service;

    public TarifasController(ITarifaService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken ct)
    {
        var result = await _service.ListarAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<TarifaDTO>>.Ok(result, "Consulta exitosa."));
    }

    [HttpGet("{tarifaGuid:guid}")]
    public async Task<IActionResult> ObtenerPorGuid(Guid tarifaGuid, CancellationToken ct)
    {
        var result = await _service.ObtenerPorGuidAsync(tarifaGuid, ct);
        return Ok(ApiResponse<TarifaDTO>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "ADMINISTRADOR")]
    public async Task<IActionResult> Crear([FromBody] TarifaCreateDTO dto, CancellationToken ct)
    {
        dto.CreadoPorUsuario = User.FindFirst("username")?.Value ?? "api_user";
        dto.CreadoDesdeIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _service.CrearAsync(dto, ct);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<TarifaDTO>.Created(result));
    }

    [HttpPut("{tarifaGuid:guid}")]
    [Authorize(Roles = "ADMINISTRADOR")]
    public async Task<IActionResult> Actualizar(Guid tarifaGuid, [FromBody] TarifaUpdateDTO dto, CancellationToken ct)
    {
        dto.ModificadoPorUsuario = User.FindFirst("username")?.Value ?? "api_user";
        dto.ModificadoDesdeIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _service.ActualizarAsync(tarifaGuid, dto, ct);
        return Ok(ApiResponse<TarifaDTO>.Ok(result, "Tarifa actualizada exitosamente."));
    }

    [HttpPatch("{tarifaGuid:guid}/desactivar")]
    [Authorize(Roles = "ADMINISTRADOR")]
    public async Task<IActionResult> Desactivar(Guid tarifaGuid, CancellationToken ct)
    {
        var usuario = User.FindFirst("username")?.Value ?? "api_user";
        await _service.DesactivarAsync(tarifaGuid, usuario, ct);
        return NoContent();
    }

    [HttpDelete("{tarifaGuid:guid}")]
    [Authorize(Roles = "ADMINISTRADOR")]
    public async Task<IActionResult> Eliminar(Guid tarifaGuid, CancellationToken ct)
    {
        var usuario = User.FindFirst("username")?.Value ?? "api_user";
        await _service.EliminarAsync(tarifaGuid, usuario, ct);
        return NoContent();
    }
}
