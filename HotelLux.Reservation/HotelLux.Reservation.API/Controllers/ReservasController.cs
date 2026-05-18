using Asp.Versioning;
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
        var maxLimite = User.IsInRole("CLIENTE") ? 200 : 500;
        var l = limite < 1
            ? (User.IsInRole("CLIENTE") ? 20 : 100)
            : Math.Min(limite, maxLimite);

        Guid? clienteFiltro = null;
        if (User.IsInRole("CLIENTE"))
        {
            var cg = ClienteSelfAccessHelper.TryGetClienteGuidClaim(User);
            if (!cg.HasValue) return Forbid();
            clienteFiltro = cg;
        }

        var page = await _service.BuscarAsync(new ReservaFiltroDTO
        {
            ClienteGuid = clienteFiltro,
            Pagina = p,
            Limite = l
        }, ct);
        return Ok(ApiResponse<PagedResultDTO<ReservaDTO>>.Ok(page));
    }

    [HttpGet("buscar")]
    public async Task<IActionResult> Buscar([FromQuery] ReservaFiltroDTO filtro, CancellationToken ct)
    {
        if (User.IsInRole("CLIENTE"))
        {
            var cg = ClienteSelfAccessHelper.TryGetClienteGuidClaim(User);
            if (!cg.HasValue) return Forbid();
            filtro.ClienteGuid = cg;
        }

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
    [Authorize(Roles = "ADMINISTRADOR,VENDEDOR,RECEPCIONISTA")]
    public async Task<IActionResult> AgregarHabitacion(
        Guid reservaGuid, [FromBody] ReservaHabitacionCreateDTO dto, CancellationToken ct)
    {
        if (!ClienteSelfAccessHelper.EsStaff(User))
            return Forbid();
        var usuario = User.Identity?.Name ?? "api_user";
        var line = await _service.AgregarHabitacionAsync(reservaGuid, dto, usuario, ct);
        return StatusCode(201, ApiResponse<ReservaHabitacionDTO>.Created(line, "Línea agregada."));
    }

    [HttpDelete("{reservaGuid:guid}/habitaciones/{lineaGuid:guid}")]
    [Authorize(Roles = "ADMINISTRADOR,VENDEDOR,RECEPCIONISTA")]
    public async Task<IActionResult> EliminarHabitacion(Guid reservaGuid, Guid lineaGuid, CancellationToken ct)
    {
        if (!ClienteSelfAccessHelper.EsStaff(User))
            return Forbid();
        var usuario = User.Identity?.Name ?? "api_user";
        await _service.EliminarHabitacionAsync(reservaGuid, lineaGuid, usuario, ct);
        return Ok(ApiResponse<string>.Ok("Línea eliminada."));
    }

    [HttpPost]
    [Authorize(Roles = "ADMINISTRADOR,VENDEDOR")]
    public async Task<IActionResult> Crear([FromBody] ReservaCreateDTO dto, CancellationToken ct)
    {
        dto.CreadoPorUsuario ??= User.Identity?.Name ?? "api_user";
        dto.CreadoDesdeIp ??= HttpContext.Connection.RemoteIpAddress?.ToString();
        var data = await _service.CrearAsync(dto, ct);
        return StatusCode(201, ApiResponse<ReservaDTO>.Created(data));
    }

    [HttpPatch("{reservaGuid:guid}/confirmar")]
    [Authorize(Roles = "ADMINISTRADOR,VENDEDOR")]
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
    [Authorize(Roles = "ADMINISTRADOR")]
    public async Task<IActionResult> Eliminar(Guid reservaGuid, CancellationToken ct)
    {
        var usuario = User.Identity?.Name ?? "api_user";
        await _service.EliminarAsync(reservaGuid, usuario, ct);
        return Ok(ApiResponse<object?>.Ok(null, "Reserva inhabilitada."));
    }
}
