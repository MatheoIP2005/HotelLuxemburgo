using Asp.Versioning;
using HotelLux.Reservation.API.Helpers;
using HotelLux.Reservation.API.Models.Common;
using HotelLux.Reservation.Business;
using HotelLux.Reservation.Business.DTOs.Common;
using HotelLux.Reservation.Business.DTOs.Reserva;
using HotelLux.Reservation.Business.DTOs.ReservaHabitacion;
using HotelLux.Reservation.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelLux.Reservation.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/internal/reservas")]
[Authorize]
public class ReservasController : ControllerBase
{
    private readonly IReservaService _service;
    public ReservasController(IReservaService service) => _service = service;

    public record CancelarRequest(string Motivo);

    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] int pagina = 1,
        [FromQuery] int limite = 100,
        CancellationToken ct = default)
    {
        var p = pagina < 1 ? 1 : pagina;
        var l = limite < 1 ? 100 : Math.Min(limite, 500);

        var page = await _service.BuscarAsync(new ReservaFiltroDTO
        {
            Pagina = p,
            Limite = l
        }, ct);
        return Ok(ApiResponse<PagedResultDTO<ReservaDTO>>.Ok(page));
    }

    [HttpGet("buscar")]
    public async Task<IActionResult> Buscar([FromQuery] ReservaFiltroDTO filtro, CancellationToken ct)
    {
        var data = await _service.BuscarAsync(filtro, ct);
        return Ok(ApiResponse<PagedResultDTO<ReservaDTO>>.Ok(data));
    }

    [HttpGet("{reservaGuid:guid}")]
    public async Task<IActionResult> ObtenerPorGuid(Guid reservaGuid, CancellationToken ct)
    {
        var data = await _service.ObtenerPorGuidAsync(reservaGuid, ct);
        if (!ClienteSelfAccessHelper.PuedeVerReservaDeCliente(User, data.ClienteGuid))
            return Forbid();
        return Ok(ApiResponse<ReservaDTO>.Ok(data));
    }

    [HttpGet("{reservaGuid:guid}/habitaciones")]
    public async Task<IActionResult> ListarHabitaciones(Guid reservaGuid, CancellationToken ct)
    {
        var res = await _service.ObtenerPorGuidAsync(reservaGuid, ct);
        if (!ClienteSelfAccessHelper.PuedeVerReservaDeCliente(User, res.ClienteGuid))
            return Forbid();
        var data = await _service.ListarHabitacionesAsync(reservaGuid, ct);
        return Ok(ApiResponse<IReadOnlyList<ReservaHabitacionDTO>>.Ok(data));
    }

    [HttpPost("{reservaGuid:guid}/habitaciones")]
    [Authorize(Roles = "ADMIN,VENDEDOR")]
    public async Task<IActionResult> AgregarHabitacion(
        Guid reservaGuid, [FromBody] ReservaHabitacionCreateDTO dto, CancellationToken ct)
    {
        if (!ClienteSelfAccessHelper.EsStaff(User))
            return Forbid();
        var usuario = User.Identity?.Name ?? "api_user";
        var line = await _service.AgregarHabitacionAsync(reservaGuid, dto, usuario, ct);
        return StatusCode(201, ApiResponse<ReservaHabitacionDTO>.Created(line, "Línea agregada."));
    }

    [HttpDelete("{reservaGuid:guid}/habitaciones/{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> EliminarHabitacion(Guid reservaGuid, int id, CancellationToken ct)
    {
        if (!ClienteSelfAccessHelper.EsStaff(User))
            return Forbid();
        var usuario = User.Identity?.Name ?? "api_user";
        await _service.EliminarHabitacionPorIdAsync(reservaGuid, id, usuario, ct);
        return Ok(ApiResponse<string>.Ok("Línea eliminada."));
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN,VENDEDOR")]
    public async Task<IActionResult> Crear([FromBody] ReservaCreateDTO dto, CancellationToken ct)
    {
        dto.CreadoPorUsuario ??= User.Identity?.Name ?? "api_user";
        dto.CreadoDesdeIp ??= HttpContext.Connection.RemoteIpAddress?.ToString();
        ReservaCreateDtoMarketplaceNormalizer.Apply(dto);
        var data = await _service.CrearAsync(dto, ct);
        return StatusCode(201, ApiResponse<ReservaDTO>.Created(data));
    }

    [HttpPatch("{reservaGuid:guid}/confirmar")]
    [Authorize(Roles = "ADMIN,VENDEDOR")]
    public async Task<IActionResult> Confirmar(Guid reservaGuid, CancellationToken ct)
    {
        var res = await _service.ObtenerPorGuidAsync(reservaGuid, ct);
        if (!ClienteSelfAccessHelper.EsStaff(User))
            return Forbid();
        var usuario = User.Identity?.Name ?? "api_user";
        var data = await _service.ConfirmarAsync(reservaGuid, usuario, ct);
        return Ok(ApiResponse<ReservaDTO>.Ok(data, "Reserva confirmada."));
    }

    [HttpPatch("{reservaGuid:guid}/cancelar")]
    public async Task<IActionResult> Cancelar(Guid reservaGuid, [FromBody] CancelarRequest request, CancellationToken ct)
    {
        var res = await _service.ObtenerPorGuidAsync(reservaGuid, ct);
        if (!ClienteSelfAccessHelper.PuedeVerReservaDeCliente(User, res.ClienteGuid))
            return Forbid();
        var usuario = User.Identity?.Name ?? "api_user";
        var data = await _service.CancelarAsync(reservaGuid, request.Motivo, usuario, ct);
        return Ok(ApiResponse<ReservaDTO>.Ok(data, "Reserva cancelada."));
    }

    [HttpDelete("{reservaGuid:guid}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Eliminar(Guid reservaGuid, CancellationToken ct)
    {
        var usuario = User.Identity?.Name ?? "api_user";
        await _service.EliminarAsync(reservaGuid, usuario, ct);
        return Ok(ApiResponse<object?>.Ok(null, "Reserva inhabilitada."));
    }
}
