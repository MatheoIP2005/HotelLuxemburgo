using Asp.Versioning;
using HotelLux.Auth.API.Models.Common;
using HotelLux.Auth.Business.DTOs.Roles;
using HotelLux.Auth.Business.DTOs.Usuarios;
using HotelLux.Auth.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelLux.Auth.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/internal/usuarios")]
[Authorize(Roles = "ADMINISTRADOR")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;

    public UsuariosController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<UsuarioDTO>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        var result = await _usuarioService.ListarAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<UsuarioDTO>>.Ok(result, "Consulta exitosa."));
    }

    [HttpGet("{usuarioGuid:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UsuarioDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorGuid(Guid usuarioGuid, CancellationToken cancellationToken)
    {
        var result = await _usuarioService.ObtenerPorGuidAsync(usuarioGuid, cancellationToken);
        return Ok(ApiResponse<UsuarioDTO>.Ok(result));
    }

    [HttpGet("{usuarioGuid:guid}/roles")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<RolDTO>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListarRoles(Guid usuarioGuid, CancellationToken cancellationToken)
    {
        var result = await _usuarioService.ListarRolesAsync(usuarioGuid, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<RolDTO>>.Ok(result, "Consulta exitosa."));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<UsuarioDTO>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Crear([FromBody] UsuarioCreateDTO dto, CancellationToken cancellationToken)
    {
        dto.CreadoPorUsuario = User.FindFirst("username")?.Value ?? "api_user";
        dto.CreadoDesdeIp = HttpContext.Connection.RemoteIpAddress?.ToString();

        var result = await _usuarioService.CrearAsync(dto, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<UsuarioDTO>.Created(result));
    }

    [HttpPut("{usuarioGuid:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UsuarioDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(Guid usuarioGuid, [FromBody] UsuarioUpdateDTO dto, CancellationToken cancellationToken)
    {
        dto.ModificadoPorUsuario = User.FindFirst("username")?.Value ?? "api_user";
        dto.ModificadoDesdeIp = HttpContext.Connection.RemoteIpAddress?.ToString();

        var result = await _usuarioService.ActualizarAsync(usuarioGuid, dto, cancellationToken);
        return Ok(ApiResponse<UsuarioDTO>.Ok(result, "Usuario actualizado exitosamente."));
    }

    [HttpPatch("{usuarioGuid:guid}/inhabilitar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Inhabilitar(Guid usuarioGuid, [FromBody] InhabilitarRequest request, CancellationToken cancellationToken)
    {
        var usuario = User.FindFirst("username")?.Value ?? "api_user";
        await _usuarioService.InhabilitarAsync(usuarioGuid, request.Motivo, usuario, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{usuarioGuid:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(Guid usuarioGuid, CancellationToken cancellationToken)
    {
        var usuario = User.FindFirst("username")?.Value ?? "api_user";
        await _usuarioService.EliminarAsync(usuarioGuid, usuario, cancellationToken);
        return NoContent();
    }

    [HttpPost("{usuarioGuid:guid}/roles")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AsignarRol(Guid usuarioGuid, [FromBody] AsignarRolRequest request, CancellationToken cancellationToken)
    {
        var usuario = User.FindFirst("username")?.Value ?? "api_user";
        await _usuarioService.AsignarRolAsync(usuarioGuid, request.RolGuid, usuario, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{usuarioGuid:guid}/roles/{idRol:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoverRol(Guid usuarioGuid, Guid idRol, CancellationToken cancellationToken)
    {
        var usuario = User.FindFirst("username")?.Value ?? "api_user";
        await _usuarioService.RemoverRolAsync(usuarioGuid, idRol, usuario, cancellationToken);
        return NoContent();
    }

    public record InhabilitarRequest(string Motivo);
    public record AsignarRolRequest(Guid RolGuid);
}
