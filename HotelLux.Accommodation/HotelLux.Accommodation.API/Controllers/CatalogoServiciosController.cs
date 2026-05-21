using Asp.Versioning;
using HotelLux.Accommodation.API.Models.Common;
using HotelLux.Accommodation.Business.DTOs.CatalogoServicio;
using HotelLux.Accommodation.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelLux.Accommodation.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/internal/catalogo-servicios")]
[Authorize]
public class CatalogoServiciosController : ControllerBase
{
    private readonly ICatalogoServicioService _service;

    public CatalogoServiciosController(ICatalogoServicioService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken ct)
    {
        var result = await _service.ListarAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<CatalogoServicioDTO>>.Ok(result, "Consulta exitosa."));
    }

    [HttpGet("{catalogoGuid:guid}")]
    public async Task<IActionResult> ObtenerPorGuid(Guid catalogoGuid, CancellationToken ct)
    {
        var result = await _service.ObtenerPorGuidAsync(catalogoGuid, ct);
        return Ok(ApiResponse<CatalogoServicioDTO>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN,VENDEDOR")]
    public async Task<IActionResult> Crear([FromBody] CatalogoServicioCreateDTO dto, CancellationToken ct)
    {
        dto.CreadoPorUsuario = User.FindFirst("username")?.Value ?? "api_user";
        dto.CreadoDesdeIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _service.CrearAsync(dto, ct);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<CatalogoServicioDTO>.Created(result));
    }

    [HttpPut("{catalogoGuid:guid}")]
    [Authorize(Roles = "ADMIN,VENDEDOR")]
    public async Task<IActionResult> Actualizar(Guid catalogoGuid, [FromBody] CatalogoServicioUpdateDTO dto, CancellationToken ct)
    {
        dto.ModificadoPorUsuario = User.FindFirst("username")?.Value ?? "api_user";
        dto.ModificadoDesdeIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _service.ActualizarAsync(catalogoGuid, dto, ct);
        return Ok(ApiResponse<CatalogoServicioDTO>.Ok(result, "Catálogo actualizado exitosamente."));
    }

    [HttpPatch("{catalogoGuid:guid}/desactivar")]
    [Authorize(Roles = "ADMIN,VENDEDOR")]
    public async Task<IActionResult> Desactivar(Guid catalogoGuid, CancellationToken ct)
    {
        var usuario = User.FindFirst("username")?.Value ?? "api_user";
        await _service.DesactivarAsync(catalogoGuid, usuario, ct);
        return NoContent();
    }

    [HttpDelete("{catalogoGuid:guid}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Eliminar(Guid catalogoGuid, CancellationToken ct)
    {
        var usuario = User.FindFirst("username")?.Value ?? "api_user";
        await _service.EliminarAsync(catalogoGuid, usuario, ct);
        return NoContent();
    }
}
