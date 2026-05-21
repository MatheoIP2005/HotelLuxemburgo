using Asp.Versioning;
using HotelLux.Accommodation.API.Models.Common;
using HotelLux.Accommodation.Business.DTOs.CatalogoServicio;
using HotelLux.Accommodation.Business.DTOs.TipoHabitacion;
using HotelLux.Accommodation.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelLux.Accommodation.API.Controllers;

/// <summary>Rutas alias <c>amenidades</c> equivalentes a <c>catalogo</c> en tipos de habitación (spec OpenAPI).</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/internal/tipos-habitacion/{tipoHabitacionGuid:guid}/amenidades")]
[Authorize]
public class TiposHabitacionAmenidadesController : ControllerBase
{
    private readonly ITipoHabitacionService _service;
    private readonly ITipoHabitacionCatalogoService _catalogoService;

    public TiposHabitacionAmenidadesController(
        ITipoHabitacionService service,
        ITipoHabitacionCatalogoService catalogoService)
    {
        _service = service;
        _catalogoService = catalogoService;
    }

    public record AsignarAmenidadRequest(Guid CatalogoGuid);

    [HttpGet]
    public async Task<IActionResult> Listar(Guid tipoHabitacionGuid, CancellationToken ct)
    {
        _ = await _service.ObtenerPorGuidAsync(tipoHabitacionGuid, ct);
        var result = await _catalogoService.ListarPorTipoAsync(tipoHabitacionGuid, ct);
        return Ok(ApiResponse<IReadOnlyList<CatalogoServicioDTO>>.Ok(result, "Consulta exitosa."));
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN,VENDEDOR")]
    public async Task<IActionResult> Asignar(Guid tipoHabitacionGuid, [FromBody] AsignarAmenidadRequest request, CancellationToken ct)
    {
        var usuario = User.FindFirst("username")?.Value ?? "api_user";
        await _catalogoService.AsignarAsync(tipoHabitacionGuid, request.CatalogoGuid, usuario, ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Remover(Guid tipoHabitacionGuid, int id, CancellationToken ct)
    {
        await _catalogoService.RemoverPorIdAsync(tipoHabitacionGuid, id, ct);
        return NoContent();
    }
}
