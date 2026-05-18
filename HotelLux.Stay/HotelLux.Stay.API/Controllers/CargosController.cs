using Asp.Versioning;
using HotelLux.Stay.API.Models.Common;
using HotelLux.Stay.Business.DTOs;
using HotelLux.Stay.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelLux.Stay.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/internal/estadias/{estadiaGuid:guid}/cargos")]
[Authorize]
public class CargosController : ControllerBase
{
    private readonly ICargoEstadiaService _service;
    public CargosController(ICargoEstadiaService service) => _service = service;

    [HttpPost]
    [Authorize(Roles = "ADMINISTRADOR,RECEPCIONISTA")]
    public async Task<IActionResult> Crear(
        Guid estadiaGuid, [FromBody] CargoEstadiaCreateDto dto, CancellationToken ct)
    {
        dto.CreadoPorUsuario ??= User.Identity?.Name ?? "api_user";
        var data = await _service.CrearAsync(estadiaGuid, dto, ct);
        return StatusCode(201, ApiResponse<CargoEstadiaDto>.Created(data, "Cargo registrado."));
    }

    [HttpGet]
    public async Task<IActionResult> Listar(Guid estadiaGuid, CancellationToken ct)
    {
        var data = await _service.ListarPorEstadiaAsync(estadiaGuid, ct);
        return Ok(ApiResponse<IReadOnlyList<CargoEstadiaDto>>.Ok(data));
    }
}
