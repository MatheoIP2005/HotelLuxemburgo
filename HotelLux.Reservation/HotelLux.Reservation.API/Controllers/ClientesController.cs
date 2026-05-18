using Asp.Versioning;
using HotelLux.Reservation.API.Models.Common;
using HotelLux.Reservation.Business;
using HotelLux.Reservation.Business.DTOs.Cliente;
using HotelLux.Reservation.Business.DTOs.Reserva;
using HotelLux.Reservation.Business.DTOs.Stay;
using HotelLux.Reservation.Business.Interfaces;
using HotelLux.Reservation.Business.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelLux.Reservation.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/internal/clientes")]
[Authorize]
public class ClientesController : ControllerBase
{
    private readonly IClienteService _service;
    private readonly IReservaService _reservas;
    private readonly IStayClient _stay;

    public ClientesController(IClienteService service, IReservaService reservas, IStayClient stay)
    {
        _service = service;
        _reservas = reservas;
        _stay = stay;
    }

    [HttpGet]
    [Authorize(Roles = "ADMINISTRADOR,RECEPCIONISTA,VENDEDOR")]
    public async Task<IActionResult> Listar([FromQuery] int pagina = 1, [FromQuery] int limite = 20, CancellationToken ct = default)
    {
        var result = await _service.ListarAsync(pagina, limite, ct);
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("{guid:guid}/reservas")]
    [Authorize(Roles = "ADMINISTRADOR,RECEPCIONISTA,VENDEDOR,CLIENTE")]
    public async Task<IActionResult> ListarReservasPorCliente(
        Guid guid, [FromQuery] int pagina = 1, [FromQuery] int limite = 20, CancellationToken ct = default)
    {
        if (!ClienteSelfAccessHelper.PuedeVerCliente(User, guid))
            return Forbid();
        var cliente = await _service.ObtenerPorGuidAsync(guid, ct);
        if (cliente is null) throw new NotFoundException("Cliente", guid);
        var page = await _reservas.BuscarAsync(new ReservaFiltroDTO
        {
            ClienteGuid = guid,
            Pagina = pagina,
            Limite = limite
        }, ct);
        return Ok(ApiResponse<object>.Ok(page));
    }

    [HttpGet("{guid:guid}/valoraciones")]
    [Authorize(Roles = "ADMINISTRADOR,RECEPCIONISTA,VENDEDOR,CLIENTE")]
    public async Task<IActionResult> ListarValoracionesPorCliente(Guid guid, CancellationToken ct = default)
    {
        if (!ClienteSelfAccessHelper.PuedeVerCliente(User, guid))
            return Forbid();
        var cliente = await _service.ObtenerPorGuidAsync(guid, ct);
        if (cliente is null) throw new NotFoundException("Cliente", guid);
        var list = await _stay.GetValoracionesByClienteAsync(guid, ct);
        return Ok(ApiResponse<IReadOnlyList<StayValoracionClienteDto>>.Ok(list));
    }

    [HttpGet("{guid:guid}")]
    public async Task<IActionResult> ObtenerPorGuid(Guid guid, CancellationToken ct = default)
    {
        if (!ClienteSelfAccessHelper.PuedeVerCliente(User, guid))
            return Forbid();
        var cliente = await _service.ObtenerPorGuidAsync(guid, ct);
        if (cliente is null) throw new NotFoundException("Cliente", guid);
        return Ok(ApiResponse<object>.Ok(cliente));
    }

    [HttpPost]
    [Authorize(Roles = "ADMINISTRADOR,RECEPCIONISTA,VENDEDOR")]
    public async Task<IActionResult> Crear([FromBody] ClienteCreateDto dto, CancellationToken ct = default)
    {
        dto.CreadoPorUsuario ??= User.Identity?.Name ?? "api_user";
        var created = await _service.CrearAsync(dto, ct);
        return StatusCode(201, ApiResponse<object>.Created(created, "Cliente registrado."));
    }

    [HttpPut("{guid:guid}")]
    [Authorize(Roles = "ADMINISTRADOR,RECEPCIONISTA")]
    public async Task<IActionResult> Actualizar(Guid guid, [FromBody] ClienteUpdateDto dto, CancellationToken ct = default)
    {
        dto.ModificadoPorUsuario ??= User.Identity?.Name ?? "api_user";
        var updated = await _service.ActualizarAsync(guid, dto, ct);
        return Ok(ApiResponse<object>.Ok(updated));
    }

    [HttpDelete("{guid:guid}")]
    [Authorize(Roles = "ADMINISTRADOR")]
    public async Task<IActionResult> Eliminar(Guid guid, CancellationToken ct = default)
    {
        var usuario = User.Identity?.Name ?? "api_user";
        await _service.EliminarLogicoAsync(guid, usuario, ct);
        return NoContent();
    }

    [HttpPatch("{guid:guid}/inhabilitar")]
    [Authorize(Roles = "ADMINISTRADOR")]
    public async Task<IActionResult> Inhabilitar(Guid guid, [FromBody] InhabilitarDto dto, CancellationToken ct = default)
    {
        if (dto is null || string.IsNullOrWhiteSpace(dto.Motivo))
            throw new ValidationException("Motivo obligatorio.", new[] { "Motivo es obligatorio." });
        var usuario = User.Identity?.Name ?? "api_user";
        await _service.InhabilitarAsync(guid, dto.Motivo, usuario, ct);
        return Ok(ApiResponse<string>.Ok("Cliente inhabilitado."));
    }
}
