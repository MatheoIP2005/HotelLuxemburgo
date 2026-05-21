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
    [Authorize(Roles = "ADMIN,VENDEDOR")]
    public async Task<IActionResult> Listar([FromQuery] int pagina = 1, [FromQuery] int limite = 20, CancellationToken ct = default)
    {
        var result = await _service.ListarAsync(pagina, limite, ct);
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("{clienteGuid:guid}/reservas")]
    [Authorize(Roles = "ADMIN,VENDEDOR")]
    public async Task<IActionResult> ListarReservasPorCliente(
        Guid clienteGuid, [FromQuery] int pagina = 1, [FromQuery] int limite = 20, CancellationToken ct = default)
    {
        if (!ClienteSelfAccessHelper.PuedeVerCliente(User, clienteGuid))
            return Forbid();
        var cliente = await _service.ObtenerPorGuidAsync(clienteGuid, ct);
        if (cliente is null) throw new NotFoundException("Cliente", clienteGuid);
        var page = await _reservas.BuscarAsync(new ReservaFiltroDTO
        {
            ClienteGuid = clienteGuid,
            Pagina = pagina,
            Limite = limite
        }, ct);
        return Ok(ApiResponse<object>.Ok(page));
    }

    [HttpGet("{clienteGuid:guid}/valoraciones")]
    [Authorize(Roles = "ADMIN,VENDEDOR")]
    public async Task<IActionResult> ListarValoracionesPorCliente(Guid clienteGuid, CancellationToken ct = default)
    {
        if (!ClienteSelfAccessHelper.PuedeVerCliente(User, clienteGuid))
            return Forbid();
        var cliente = await _service.ObtenerPorGuidAsync(clienteGuid, ct);
        if (cliente is null) throw new NotFoundException("Cliente", clienteGuid);
        var list = await _stay.GetValoracionesByClienteAsync(clienteGuid, ct);
        return Ok(ApiResponse<IReadOnlyList<StayValoracionClienteDto>>.Ok(list));
    }

    [HttpGet("{clienteGuid:guid}")]
    public async Task<IActionResult> ObtenerPorGuid(Guid clienteGuid, CancellationToken ct = default)
    {
        if (!ClienteSelfAccessHelper.PuedeVerCliente(User, clienteGuid))
            return Forbid();
        var cliente = await _service.ObtenerPorGuidAsync(clienteGuid, ct);
        if (cliente is null) throw new NotFoundException("Cliente", clienteGuid);
        return Ok(ApiResponse<object>.Ok(cliente));
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN,VENDEDOR")]
    public async Task<IActionResult> Crear([FromBody] ClienteCreateDto dto, CancellationToken ct = default)
    {
        dto.CreadoPorUsuario ??= User.Identity?.Name ?? "api_user";
        var created = await _service.CrearAsync(dto, ct);
        return StatusCode(201, ApiResponse<object>.Created(created, "Cliente registrado."));
    }

    [HttpPut("{clienteGuid:guid}")]
    [Authorize(Roles = "ADMIN,VENDEDOR")]
    public async Task<IActionResult> Actualizar(Guid clienteGuid, [FromBody] ClienteUpdateDto dto, CancellationToken ct = default)
    {
        dto.ModificadoPorUsuario ??= User.Identity?.Name ?? "api_user";
        var updated = await _service.ActualizarAsync(clienteGuid, dto, ct);
        return Ok(ApiResponse<object>.Ok(updated));
    }

    [HttpDelete("{clienteGuid:guid}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Eliminar(Guid clienteGuid, CancellationToken ct = default)
    {
        var usuario = User.Identity?.Name ?? "api_user";
        await _service.EliminarLogicoAsync(clienteGuid, usuario, ct);
        return NoContent();
    }

    [HttpPatch("{clienteGuid:guid}/inhabilitar")]
    [Authorize(Roles = "ADMIN,VENDEDOR")]
    public async Task<IActionResult> Inhabilitar(Guid clienteGuid, [FromBody] InhabilitarDto dto, CancellationToken ct = default)
    {
        if (dto is null || string.IsNullOrWhiteSpace(dto.Motivo))
            throw new ValidationException("Motivo obligatorio.", new[] { "Motivo es obligatorio." });
        var usuario = User.Identity?.Name ?? "api_user";
        await _service.InhabilitarAsync(clienteGuid, dto.Motivo, usuario, ct);
        return Ok(ApiResponse<string>.Ok("Cliente inhabilitado."));
    }
}
