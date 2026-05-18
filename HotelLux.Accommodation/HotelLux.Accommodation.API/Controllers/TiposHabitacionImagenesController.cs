using Asp.Versioning;
using HotelLux.Accommodation.API.Models.Common;
using HotelLux.Accommodation.Business.DTOs.TipoHabitacionImagen;
using HotelLux.Accommodation.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelLux.Accommodation.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/internal/tipos-habitacion/{tipoHabitacionGuid:guid}/imagenes")]
[Authorize]
public class TiposHabitacionImagenesController : ControllerBase
{
    private readonly ITipoHabitacionImagenService _service;

    public TiposHabitacionImagenesController(ITipoHabitacionImagenService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Listar(Guid tipoHabitacionGuid, CancellationToken ct)
    {
        var result = await _service.ListarPorTipoAsync(tipoHabitacionGuid, ct);
        return Ok(ApiResponse<IReadOnlyList<TipoHabitacionImagenDTO>>.Ok(result, "Consulta exitosa."));
    }

    [HttpPost]
    [Authorize(Roles = "ADMINISTRADOR")]
    public async Task<IActionResult> Crear(Guid tipoHabitacionGuid, [FromBody] TipoHabitacionImagenCreateDTO dto, CancellationToken ct)
    {
        dto.CreadoPorUsuario = User.FindFirst("username")?.Value ?? "api_user";
        var result = await _service.CrearAsync(tipoHabitacionGuid, dto, ct);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<TipoHabitacionImagenDTO>.Created(result));
    }

    [HttpDelete("{idImagen:int}")]
    [Authorize(Roles = "ADMINISTRADOR")]
    public async Task<IActionResult> Eliminar(Guid tipoHabitacionGuid, int idImagen, CancellationToken ct)
    {
        await _service.EliminarAsync(tipoHabitacionGuid, idImagen, ct);
        return NoContent();
    }
}
