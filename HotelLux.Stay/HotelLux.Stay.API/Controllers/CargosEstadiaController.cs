using Asp.Versioning;
using HotelLux.Stay.API.Models.Common;
using HotelLux.Stay.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelLux.Stay.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/internal/cargos-estadia")]
[Authorize]
public class CargosEstadiaController : ControllerBase
{
    private readonly ICargoEstadiaService _service;
    public CargosEstadiaController(ICargoEstadiaService service) => _service = service;

    [HttpGet("{cargoGuid:guid}")]
    public async Task<IActionResult> Obtener(Guid cargoGuid, CancellationToken ct)
    {
        var data = await _service.ObtenerPorGuidAsync(cargoGuid, ct);
        return Ok(ApiResponse<object>.Ok(data));
    }

    [HttpPatch("{cargoGuid:guid}/anular")]
    [Authorize(Roles = "ADMINISTRADOR,RECEPCIONISTA")]
    public async Task<IActionResult> Anular(Guid cargoGuid, CancellationToken ct)
    {
        var usuario = User.Identity?.Name ?? "api_user";
        await _service.AnularAsync(cargoGuid, usuario, ct);
        return Ok(ApiResponse<string>.Ok("Cargo anulado."));
    }
}
