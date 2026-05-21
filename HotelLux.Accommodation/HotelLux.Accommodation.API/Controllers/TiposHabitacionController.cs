using Asp.Versioning;
using HotelLux.Accommodation.API.Models.Common;
using HotelLux.Accommodation.Business.DTOs.CatalogoServicio;
using HotelLux.Accommodation.Business.DTOs.TipoHabitacion;
using HotelLux.Accommodation.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelLux.Accommodation.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/internal/tipos-habitacion")]
[Authorize]
public class TiposHabitacionController : ControllerBase
{
    private readonly ITipoHabitacionService _service;
    private readonly ITipoHabitacionCatalogoService _catalogoService;

    public TiposHabitacionController(ITipoHabitacionService service, ITipoHabitacionCatalogoService catalogoService)
    {
        _service = service;
        _catalogoService = catalogoService;
    }

    public record AsignarCatalogoRequest(Guid CatalogoGuid);

    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken ct)
    {
        var result = await _service.ListarAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<TipoHabitacionDTO>>.Ok(result, "Consulta exitosa."));
    }

    [HttpGet("{tipoHabitacionGuid:guid}")]
    public async Task<IActionResult> ObtenerPorGuid(Guid tipoHabitacionGuid, CancellationToken ct)
    {
        var result = await _service.ObtenerPorGuidAsync(tipoHabitacionGuid, ct);
        return Ok(ApiResponse<TipoHabitacionDTO>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN,VENDEDOR")]
    public async Task<IActionResult> Crear([FromBody] TipoHabitacionCreateDTO dto, CancellationToken ct)
    {
        dto.CreadoPorUsuario = User.FindFirst("username")?.Value ?? "api_user";
        dto.CreadoDesdeIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _service.CrearAsync(dto, ct);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<TipoHabitacionDTO>.Created(result));
    }

    [HttpPut("{tipoHabitacionGuid:guid}")]
    [Authorize(Roles = "ADMIN,VENDEDOR")]
    public async Task<IActionResult> Actualizar(Guid tipoHabitacionGuid, [FromBody] TipoHabitacionUpdateDTO dto, CancellationToken ct)
    {
        dto.ModificadoPorUsuario = User.FindFirst("username")?.Value ?? "api_user";
        dto.ModificadoDesdeIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _service.ActualizarAsync(tipoHabitacionGuid, dto, ct);
        return Ok(ApiResponse<TipoHabitacionDTO>.Ok(result, "Tipo de habitación actualizado exitosamente."));
    }

    [HttpPatch("{tipoHabitacionGuid:guid}/inhabilitar")]
    [Authorize(Roles = "ADMIN,VENDEDOR")]
    public async Task<IActionResult> Inhabilitar(Guid tipoHabitacionGuid, CancellationToken ct)
    {
        var usuario = User.FindFirst("username")?.Value ?? "api_user";
        await _service.InhabilitarAsync(tipoHabitacionGuid, usuario, ct);
        return NoContent();
    }

    [HttpDelete("{tipoHabitacionGuid:guid}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Eliminar(Guid tipoHabitacionGuid, CancellationToken ct)
    {
        var usuario = User.FindFirst("username")?.Value ?? "api_user";
        await _service.EliminarAsync(tipoHabitacionGuid, usuario, ct);
        return NoContent();
    }

    [HttpGet("{tipoHabitacionGuid:guid}/catalogo")]
    public async Task<IActionResult> ListarCatalogo(Guid tipoHabitacionGuid, CancellationToken ct)
    {
        var result = await _catalogoService.ListarPorTipoAsync(tipoHabitacionGuid, ct);
        return Ok(ApiResponse<IReadOnlyList<CatalogoServicioDTO>>.Ok(result, "Consulta exitosa."));
    }

    [HttpPost("{tipoHabitacionGuid:guid}/catalogo")]
    [Authorize(Roles = "ADMIN,VENDEDOR")]
    public async Task<IActionResult> AsignarCatalogo(Guid tipoHabitacionGuid, [FromBody] AsignarCatalogoRequest request, CancellationToken ct)
    {
        var usuario = User.FindFirst("username")?.Value ?? "api_user";
        await _catalogoService.AsignarAsync(tipoHabitacionGuid, request.CatalogoGuid, usuario, ct);
        return NoContent();
    }

    [HttpDelete("{tipoHabitacionGuid:guid}/catalogo/{catalogoGuid:guid}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> RemoverCatalogo(Guid tipoHabitacionGuid, Guid catalogoGuid, CancellationToken ct)
    {
        await _catalogoService.RemoverAsync(tipoHabitacionGuid, catalogoGuid, ct);
        return NoContent();
    }
}
