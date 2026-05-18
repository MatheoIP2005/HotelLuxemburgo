using Asp.Versioning;
using HotelLux.Stay.API.Models.Common;
using HotelLux.Stay.Business.DTOs;
using HotelLux.Stay.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelLux.Stay.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/internal/estadias")]
[Authorize]
public class EstadiasController : ControllerBase
{
    private readonly IEstadiaService _estadia;
    public EstadiasController(IEstadiaService estadia) => _estadia = estadia;

    /// <summary>Lista estadías con filtros opcionales de estado y sucursal.</summary>
    [HttpGet]
    [Authorize(Roles = "ADMINISTRADOR,RECEPCIONISTA")]
    public async Task<IActionResult> Listar(
        [FromQuery] string? estado,
        [FromQuery] Guid? sucursalGuid,
        [FromQuery] int pagina = 1,
        [FromQuery] int limite = 20,
        CancellationToken ct = default)
    {
        var resultado = await _estadia.ListarAsync(estado, sucursalGuid, pagina, limite, ct);
        return Ok(ApiResponse<object>.Ok(resultado));
    }

    /// <summary>Registra el check-in de una reserva confirmada.</summary>
    [HttpPost("check-in")]
    [Authorize(Roles = "ADMINISTRADOR,VENDEDOR,RECEPCIONISTA")]
    public async Task<IActionResult> CheckIn([FromBody] CheckInDto dto, CancellationToken ct)
    {
        dto.CreadoPorUsuario ??= User.Identity?.Name ?? "api_user";
        var data = await _estadia.CheckInAsync(dto, ct);
        return StatusCode(201, ApiResponse<EstadiaDto>.Created(data, "Check-in registrado."));
    }

    /// <summary>Alias de check-in con la reserva en la ruta.</summary>
    [HttpPost("checkin/{reservaGuid:guid}")]
    [Authorize(Roles = "ADMINISTRADOR,VENDEDOR,RECEPCIONISTA")]
    public async Task<IActionResult> CheckInPorReserva(
        Guid reservaGuid, [FromBody] CheckInPorReservaBodyDto? body, CancellationToken ct)
    {
        var dto = new CheckInDto
        {
            ReservaGuid = reservaGuid,
            ReservaHabitacionGuid = body?.ReservaHabitacionGuid,
            CreadoPorUsuario = User.Identity?.Name ?? "api_user"
        };
        var data = await _estadia.CheckInAsync(dto, ct);
        return StatusCode(201, ApiResponse<EstadiaDto>.Created(data, "Check-in registrado."));
    }

    /// <summary>Registra el check-out de una estadía activa y genera factura final.</summary>
    [HttpPatch("{estadiaGuid:guid}/check-out")]
    [Authorize(Roles = "ADMINISTRADOR,VENDEDOR,RECEPCIONISTA")]
    public async Task<IActionResult> CheckOut(Guid estadiaGuid, CancellationToken ct)
    {
        var usuario = User.Identity?.Name ?? "api_user";
        var data = await _estadia.CheckOutAsync(estadiaGuid, usuario, ct);
        return Ok(ApiResponse<EstadiaDto>.Ok(data, "Check-out registrado."));
    }

    /// <summary>Alias spec: PATCH .../checkout (mismo comportamiento que check-out).</summary>
    [HttpPatch("{estadiaGuid:guid}/checkout")]
    [Authorize(Roles = "ADMINISTRADOR,VENDEDOR,RECEPCIONISTA")]
    public Task<IActionResult> CheckOutSpecAlias(Guid estadiaGuid, CancellationToken ct)
        => CheckOut(estadiaGuid, ct);

    /// <summary>Alias de check-out con el GUID de la estadía en el cuerpo.</summary>
    [HttpPatch("checkout")]
    [Authorize(Roles = "ADMINISTRADOR,VENDEDOR,RECEPCIONISTA")]
    public async Task<IActionResult> CheckOutPorBody([FromBody] CheckoutPorBodyDto body, CancellationToken ct)
    {
        var usuario = User.Identity?.Name ?? "api_user";
        var data = await _estadia.CheckOutAsync(body.EstadiaGuid, usuario, ct);
        return Ok(ApiResponse<EstadiaDto>.Ok(data, "Check-out registrado."));
    }

    /// <summary>Obtiene una estadía por su GUID.</summary>
    [HttpGet("{estadiaGuid:guid}")]
    [Authorize(Roles = "ADMINISTRADOR,RECEPCIONISTA,VENDEDOR")]
    public async Task<IActionResult> ObtenerPorGuid(Guid estadiaGuid, CancellationToken ct)
    {
        var data = await _estadia.ObtenerPorGuidAsync(estadiaGuid, ct);
        if (data is null)
            return NotFound(ApiResponse<string>.Error("Estadía no encontrada."));
        return Ok(ApiResponse<EstadiaDto>.Ok(data));
    }

    /// <summary>Marca o desmarca la habitación para mantenimiento al terminar la estadía.</summary>
    [HttpPatch("{estadiaGuid:guid}/mantenimiento")]
    [Authorize(Roles = "ADMINISTRADOR,RECEPCIONISTA")]
    public async Task<IActionResult> MarcarMantenimiento(Guid estadiaGuid, CancellationToken ct)
    {
        var usuario = User.Identity?.Name ?? "api_user";
        await _estadia.MarcarMantenimientoAsync(estadiaGuid, usuario, ct);
        return Ok(ApiResponse<string>.Ok("Estado de mantenimiento actualizado."));
    }
}
