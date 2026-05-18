using Asp.Versioning;
using HotelLux.Auth.API.Models.Common;
using HotelLux.Auth.Business.DTOs.Roles;
using HotelLux.Auth.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelLux.Auth.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/internal/roles")]
[Authorize(Roles = "ADMINISTRADOR")]
public class RolesController : ControllerBase
{
    private readonly IRolService _rolService;

    public RolesController(IRolService rolService)
    {
        _rolService = rolService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<RolDTO>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        var result = await _rolService.ListarAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<RolDTO>>.Ok(result, "Consulta exitosa."));
    }

    [HttpGet("{rolGuid:guid}")]
    [ProducesResponseType(typeof(ApiResponse<RolDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorGuid(Guid rolGuid, CancellationToken cancellationToken)
    {
        var result = await _rolService.ObtenerPorGuidAsync(rolGuid, cancellationToken);
        return Ok(ApiResponse<RolDTO>.Ok(result));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<RolDTO>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Crear([FromBody] RolCreateDTO dto, CancellationToken cancellationToken)
    {
        dto.CreadoPorUsuario = User.FindFirst("username")?.Value ?? "api_user";
        dto.CreadoDesdeIp = HttpContext.Connection.RemoteIpAddress?.ToString();

        var result = await _rolService.CrearAsync(dto, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<RolDTO>.Created(result));
    }

    [HttpPut("{rolGuid:guid}")]
    [ProducesResponseType(typeof(ApiResponse<RolDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(Guid rolGuid, [FromBody] RolUpdateDTO dto, CancellationToken cancellationToken)
    {
        dto.ModificadoPorUsuario = User.FindFirst("username")?.Value ?? "api_user";
        dto.ModificadoDesdeIp = HttpContext.Connection.RemoteIpAddress?.ToString();

        var result = await _rolService.ActualizarAsync(rolGuid, dto, cancellationToken);
        return Ok(ApiResponse<RolDTO>.Ok(result, "Rol actualizado exitosamente."));
    }

    [HttpPatch("{rolGuid:guid}/inhabilitar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Inhabilitar(Guid rolGuid, CancellationToken cancellationToken)
    {
        var usuario = User.FindFirst("username")?.Value ?? "api_user";
        await _rolService.InhabilitarAsync(rolGuid, usuario, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{rolGuid:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(Guid rolGuid, CancellationToken cancellationToken)
    {
        var usuario = User.FindFirst("username")?.Value ?? "api_user";
        await _rolService.EliminarAsync(rolGuid, usuario, cancellationToken);
        return NoContent();
    }

    [HttpPost("{rolGuid:guid}/permisos")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult AsignarPermiso(Guid rolGuid, [FromBody] object request)
    {
        return NoContent();
    }

    [HttpDelete("{rolGuid:guid}/permisos/{idPermiso:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult RemoverPermiso(Guid rolGuid, int idPermiso)
    {
        return NoContent();
    }
}
