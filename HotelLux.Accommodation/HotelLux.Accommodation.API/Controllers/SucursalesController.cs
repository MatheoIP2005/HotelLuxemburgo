using Asp.Versioning;
using HotelLux.Accommodation.API.Models.Common;
using HotelLux.Accommodation.API.Services;
using HotelLux.Accommodation.Business.DTOs.Sucursal;
using HotelLux.Accommodation.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelLux.Accommodation.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/internal/sucursales")]
[Authorize]
public class SucursalesController : ControllerBase
{
    private readonly ISucursalService _service;
    private readonly IStayPublicClient _stay;

    public SucursalesController(ISucursalService service, IStayPublicClient stay)
    {
        _service = service;
        _stay = stay;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SucursalDTO>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken ct)
    {
        var result = await _service.ListarAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<SucursalDTO>>.Ok(result, "Consulta exitosa."));
    }

    [HttpPatch("{sucursalGuid:guid}/politicas")]
    [Authorize(Roles = "ADMIN,VENDEDOR")]
    [ProducesResponseType(typeof(ApiResponse<SucursalDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ActualizarPoliticas(
        Guid sucursalGuid, [FromBody] SucursalPoliticasPatchDTO? dto, CancellationToken ct)
    {
        if (dto is null)
            return BadRequest(ApiErrorResponse.Fail(StatusCodes.Status400BadRequest, "Cuerpo requerido."));
        dto.ModificadoPorUsuario = User.FindFirst("username")?.Value ?? "api_user";
        dto.ModificadoDesdeIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _service.ActualizarPoliticasAsync(sucursalGuid, dto, ct);
        return Ok(ApiResponse<SucursalDTO>.Ok(result, "Políticas actualizadas."));
    }

    [HttpGet("{sucursalGuid:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SucursalDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorGuid(Guid sucursalGuid, CancellationToken ct)
    {
        var result = await _service.ObtenerPorGuidAsync(sucursalGuid, ct);
        return Ok(ApiResponse<SucursalDTO>.Ok(result));
    }

    [HttpGet("{sucursalGuid:guid}/resumen-rating")]
    [ProducesResponseType(typeof(ApiResponse<StayRatingSummary>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ResumenRating(Guid sucursalGuid, CancellationToken ct)
    {
        var resumen = await _stay.GetRatingSummaryAsync(sucursalGuid, ct)
            ?? new StayRatingSummary { TieneResenas = false };
        return Ok(ApiResponse<StayRatingSummary>.Ok(resumen, "Consulta exitosa."));
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN,VENDEDOR")]
    [ProducesResponseType(typeof(ApiResponse<SucursalDTO>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Crear([FromBody] SucursalCreateDTO dto, CancellationToken ct)
    {
        dto.CreadoPorUsuario = User.FindFirst("username")?.Value ?? "api_user";
        dto.CreadoDesdeIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _service.CrearAsync(dto, ct);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<SucursalDTO>.Created(result));
    }

    [HttpPut("{sucursalGuid:guid}")]
    [Authorize(Roles = "ADMIN,VENDEDOR")]
    [ProducesResponseType(typeof(ApiResponse<SucursalDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(Guid sucursalGuid, [FromBody] SucursalUpdateDTO dto, CancellationToken ct)
    {
        dto.ModificadoPorUsuario = User.FindFirst("username")?.Value ?? "api_user";
        dto.ModificadoDesdeIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _service.ActualizarAsync(sucursalGuid, dto, ct);
        return Ok(ApiResponse<SucursalDTO>.Ok(result, "Sucursal actualizada exitosamente."));
    }

    [HttpPatch("{sucursalGuid:guid}/inhabilitar")]
    [Authorize(Roles = "ADMIN,VENDEDOR")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Inhabilitar(Guid sucursalGuid, CancellationToken ct)
    {
        var usuario = User.FindFirst("username")?.Value ?? "api_user";
        await _service.InhabilitarAsync(sucursalGuid, usuario, ct);
        return NoContent();
    }

    [HttpDelete("{sucursalGuid:guid}")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(Guid sucursalGuid, CancellationToken ct)
    {
        var usuario = User.FindFirst("username")?.Value ?? "api_user";
        await _service.EliminarAsync(sucursalGuid, usuario, ct);
        return NoContent();
    }
}
