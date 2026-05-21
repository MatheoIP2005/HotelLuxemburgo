using Asp.Versioning;
using HotelLux.Finance.API.Models.Common;
using HotelLux.Finance.Business.DTOs;
using HotelLux.Finance.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelLux.Finance.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/internal/pagos")]
[Authorize]
public class PagosController : ControllerBase
{
    private readonly IPagoService _pagos;

    public PagosController(IPagoService pagos) => _pagos = pagos;

    [HttpPost]
    public async Task<IActionResult> Registrar([FromBody] PagoCreateDto dto, CancellationToken ct)
    {
        var usuario = dto.CreadoPorUsuario ?? User.Identity?.Name ?? "api_user";
        var pago = await _pagos.RegistrarAsync(dto.FacturaGuid, dto.Monto, dto.MetodoPago, usuario, ct);
        return StatusCode(201, ApiResponse<object>.Created(new { pago.PagoGuid, dto.FacturaGuid, dto.Monto }, "Pago registrado."));
    }

    [HttpPut("{pagoGuid:guid}/aprobar")]
    [Authorize(Roles = "ADMIN,VENDEDOR")]
    public async Task<IActionResult> Aprobar(Guid pagoGuid, CancellationToken ct)
    {
        var usuario = User.Identity?.Name ?? "api_user";
        var ok = await _pagos.AprobarAsync(pagoGuid, usuario, ct);
        if (!ok) return NotFound(ApiResponse<string>.Error("Pago no encontrado o ya procesado."));
        return Ok(ApiResponse<string>.Ok("Pago aprobado y saldo actualizado."));
    }

    [HttpPatch("{pagoGuid:guid}/estado")]
    [Authorize(Roles = "ADMIN,VENDEDOR")]
    public async Task<IActionResult> CambiarEstado(Guid pagoGuid, [FromBody] PagoEstadoDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto?.NuevoEstado))
            return BadRequest(ApiResponse<string>.Error("Estado obligatorio."));
        var usuario = User.Identity?.Name ?? "api_user";
        try
        {
            var ok = await _pagos.ActualizarEstadoAsync(pagoGuid, dto.NuevoEstado, usuario, ct);
            if (!ok) return NotFound(ApiResponse<string>.Error("Pago no encontrado."));
            return Ok(ApiResponse<string>.Ok("Estado de pago actualizado."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.Error(ex.Message));
        }
    }

    [HttpGet("{pagoGuid:guid}")]
    public async Task<IActionResult> ObtenerPorGuid(Guid pagoGuid, CancellationToken ct)
    {
        var pago = await _pagos.ObtenerPorGuidAsync(pagoGuid, ct);
        if (pago is null) return NotFound(ApiResponse<string>.Error("Pago no encontrado."));
        return Ok(ApiResponse<object>.Ok(pago));
    }

    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] Guid? facturaGuid,
        [FromQuery] Guid? reservaGuid,
        [FromQuery] string? estado,
        [FromQuery] string? metodo,
        [FromQuery] DateTimeOffset? fechaDesde,
        [FromQuery] DateTimeOffset? fechaHasta,
        [FromQuery] int limite = 200,
        CancellationToken ct = default)
    {
        if (facturaGuid.HasValue && reservaGuid is null && string.IsNullOrWhiteSpace(estado)
            && string.IsNullOrWhiteSpace(metodo) && !fechaDesde.HasValue && !fechaHasta.HasValue)
        {
            var soloFactura = await _pagos.ListarPorFacturaAsync(facturaGuid.Value, ct);
            return Ok(ApiResponse<object>.Ok(soloFactura));
        }

        var pagos = await _pagos.ListarFiltradoAsync(
            facturaGuid, reservaGuid, estado, metodo, fechaDesde, fechaHasta, limite, ct);
        return Ok(ApiResponse<object>.Ok(pagos));
    }
}
