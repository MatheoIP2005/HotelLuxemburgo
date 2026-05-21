using Asp.Versioning;
using HotelLux.Accommodation.API.Models.Common;
using HotelLux.Accommodation.Business.DTOs.SucursalImagen;
using HotelLux.Accommodation.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelLux.Accommodation.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/internal/sucursales/{sucursalGuid:guid}/imagenes")]
[Authorize]
public class SucursalImagenesController : ControllerBase
{
    private readonly ISucursalImagenService _service;

    public SucursalImagenesController(ISucursalImagenService service) => _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SucursalImagenDTO>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(Guid sucursalGuid, CancellationToken ct)
    {
        var result = await _service.ListarPorSucursalAsync(sucursalGuid, ct);
        return Ok(ApiResponse<IReadOnlyList<SucursalImagenDTO>>.Ok(result, "Consulta exitosa."));
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN,VENDEDOR")]
    [ProducesResponseType(typeof(ApiResponse<SucursalImagenDTO>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Crear(Guid sucursalGuid, [FromBody] SucursalImagenCreateDTO dto, CancellationToken ct)
    {
        dto.CreadoPorUsuario = User.FindFirst("username")?.Value ?? "api_user";
        var result = await _service.CrearAsync(sucursalGuid, dto, ct);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<SucursalImagenDTO>.Created(result));
    }

    [HttpDelete("{idSucursalImagen:int}")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> EliminarPorId(Guid sucursalGuid, int idSucursalImagen, CancellationToken ct)
    {
        await _service.EliminarPorIdSucursalImagenAsync(sucursalGuid, idSucursalImagen, ct);
        return NoContent();
    }
}
