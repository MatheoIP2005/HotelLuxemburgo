using Asp.Versioning;
using HotelLux.Stay.API.Models.Common;
using HotelLux.Stay.Business.DTOs;
using HotelLux.Stay.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelLux.Stay.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/internal/valoraciones")]
[Authorize]
public class ValoracionesController : ControllerBase
{
    private readonly IValoracionService _service;
    public ValoracionesController(IValoracionService service) => _service = service;

    [HttpGet]
    [Authorize(Roles = "ADMIN,VENDEDOR")]
    public async Task<IActionResult> ListarPaginado(
        [FromQuery] int pagina = 1, [FromQuery] int limite = 20, CancellationToken ct = default)
    {
        var (items, total) = await _service.ListarPaginadoAsync(pagina, limite, ct);
        return Ok(ApiResponse<object>.Ok(new { items, total, pagina, limite }));
    }

    [HttpGet("estadia/{estadiaGuid:guid}")]
    public async Task<IActionResult> ListarPorEstadia(Guid estadiaGuid, CancellationToken ct)
    {
        var data = await _service.ListarPorEstadiaAsync(estadiaGuid, ct);
        return Ok(ApiResponse<IReadOnlyList<ValoracionDto>>.Ok(data));
    }

    [HttpGet("{valoracionGuid:guid}")]
    public async Task<IActionResult> ObtenerPorGuid(Guid valoracionGuid, CancellationToken ct)
    {
        var data = await _service.ObtenerPorGuidAsync(valoracionGuid, ct);
        if (data is null)
            return NotFound(ApiResponse<string>.Error("Valoración no encontrada."));
        return Ok(ApiResponse<ValoracionDto>.Ok(data));
    }

    [HttpPatch("{valoracionGuid:guid}/moderar")]
    [Authorize(Roles = "ADMIN,VENDEDOR")]
    public async Task<IActionResult> Moderar(Guid valoracionGuid, CancellationToken ct)
    {
        var usuario = User.Identity?.Name ?? "api_user";
        await _service.ModerarOcultarAsync(valoracionGuid, usuario, ct);
        return Ok(ApiResponse<string>.Ok("Valoración ocultada."));
    }

    [HttpPatch("{valoracionGuid:guid}/respuesta")]
    [Authorize(Roles = "ADMIN,VENDEDOR")]
    public async Task<IActionResult> Responder(
        Guid valoracionGuid, [FromBody] ValoracionResponderDto dto, CancellationToken ct)
    {
        var texto = !string.IsNullOrWhiteSpace(dto.RespuestaHotel) ? dto.RespuestaHotel! : dto.Respuesta;
        if (string.IsNullOrWhiteSpace(texto))
            return BadRequest(ApiResponse<string>.Error("Se requiere respuesta o respuestaHotel."));

        var usuario = User.Identity?.Name ?? "api_user";
        await _service.ResponderAsync(valoracionGuid, texto.Trim(), usuario, ct);
        var data = await _service.ObtenerPorGuidAsync(valoracionGuid, ct);
        return Ok(ApiResponse<ValoracionDto>.Ok(data!, "Respuesta registrada."));
    }

    [HttpPatch("{valoracionGuid:guid}/responder")]
    [Authorize(Roles = "ADMIN,VENDEDOR")]
    public Task<IActionResult> ResponderAlias(
        Guid valoracionGuid, [FromBody] ValoracionResponderDto dto, CancellationToken ct)
        => Responder(valoracionGuid, dto, ct);

    [HttpDelete("{valoracionGuid:guid}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Eliminar(Guid valoracionGuid, CancellationToken ct)
    {
        var usuario = User.Identity?.Name ?? "api_user";
        await _service.EliminarAsync(valoracionGuid, usuario, ct);
        return NoContent();
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN,VENDEDOR")]
    public async Task<IActionResult> Crear([FromBody] ValoracionCreateDto dto, CancellationToken ct)
    {
        dto.CreadoPorUsuario ??= User.Identity?.Name ?? "api_user";
        var data = await _service.CrearAsync(dto, ct);
        return StatusCode(201, ApiResponse<ValoracionDto>.Created(data, "Valoración registrada."));
    }
}
