using Asp.Versioning;
using HotelLux.Finance.API.Models.Common;
using HotelLux.Finance.Business.DTOs;
using HotelLux.Finance.Business.Exceptions;
using HotelLux.Finance.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelLux.Finance.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/internal/facturas")]
[Authorize]
public class FacturasController : ControllerBase
{
    private readonly IFacturaService _facturas;
    private readonly IPagoService _pagos;

    public FacturasController(IFacturaService facturas, IPagoService pagos)
    {
        _facturas = facturas;
        _pagos = pagos;
    }

    [HttpGet("reserva/{idReserva:guid}")]
    public async Task<IActionResult> ListarPorReserva(Guid idReserva, CancellationToken ct)
    {
        var data = await _facturas.ListarPorReservaGuidAsync(idReserva, ct);
        return Ok(ApiResponse<IReadOnlyList<FacturaDto>>.Ok(data));
    }

    [HttpPost("generar-reserva/{reservaGuid:guid}")]
    [Authorize(Roles = "ADMIN,VENDEDOR")]
    public async Task<IActionResult> GenerarReserva(
        Guid reservaGuid, [FromBody] GenerarFacturaRequestDto body, CancellationToken ct)
    {
        if (body.Items.Count == 0)
            return BadRequest(ApiResponse<string>.Error("Items no puede estar vacío."));
        var usuario = User.Identity?.Name ?? "api_user";
        try
        {
            var created = await _facturas.GenerarConLineasAsync(
                "RESERVA", reservaGuid, body.ClienteGuid, body.SucursalGuid, body.Items, usuario, ct);
            return StatusCode(201, ApiResponse<FacturaDto>.Created(created, "Factura RESERVA generada."));
        }
        catch (ValidationException ex)
        {
            return BadRequest(ApiResponse<string>.Error(ex.Message));
        }
    }

    [HttpPost("generar-final/{reservaGuid:guid}")]
    [Authorize(Roles = "ADMIN,VENDEDOR")]
    public async Task<IActionResult> GenerarFinal(
        Guid reservaGuid, [FromBody] GenerarFacturaRequestDto body, CancellationToken ct)
    {
        if (body.Items.Count == 0)
            return BadRequest(ApiResponse<string>.Error("Items no puede estar vacío."));
        var usuario = User.Identity?.Name ?? "api_user";
        try
        {
            var created = await _facturas.GenerarConLineasAsync(
                "FINAL", reservaGuid, body.ClienteGuid, body.SucursalGuid, body.Items, usuario, ct);
            return StatusCode(201, ApiResponse<FacturaDto>.Created(created, "Factura FINAL generada."));
        }
        catch (ValidationException ex)
        {
            return BadRequest(ApiResponse<string>.Error(ex.Message));
        }
    }

    /// <summary>Genera factura FINAL y registra un pago en efectivo simulado aprobado en cadena.</summary>
    [HttpPost("final-y-pago-simulado/{reservaGuid:guid}")]
    [Authorize(Roles = "ADMIN,VENDEDOR")]
    public async Task<IActionResult> FinalYPagoSimulado(
        Guid reservaGuid, [FromBody] GenerarFacturaRequestDto body, CancellationToken ct)
    {
        if (body.Items.Count == 0)
            return BadRequest(ApiResponse<string>.Error("Items no puede estar vacío."));
        var usuario = User.Identity?.Name ?? "api_user";
        try
        {
            var factura = await _facturas.GenerarConLineasAsync(
                "FINAL", reservaGuid, body.ClienteGuid, body.SucursalGuid, body.Items, usuario, ct);
            if (factura.SaldoPendiente <= 0)
                return StatusCode(201, ApiResponse<object>.Created(new { factura, pago = (object?)null },
                    "Factura sin saldo pendiente."));
            var pago = await _pagos.RegistrarAsync(
                factura.FacturaGuid, factura.SaldoPendiente, "EFECTIVO_SIMULADO", usuario, ct);
            await _pagos.AprobarAsync(pago.PagoGuid, usuario, ct);
            var facturaActualizada = await _facturas.ObtenerPorGuidAsync(factura.FacturaGuid, ct);
            return StatusCode(201, ApiResponse<object>.Created(new { factura = facturaActualizada, pago },
                "Factura final y pago simulado registrados."));
        }
        catch (ValidationException ex)
        {
            return BadRequest(ApiResponse<string>.Error(ex.Message));
        }
    }

    [HttpGet("{facturaGuid:guid}/detalle")]
    public async Task<IActionResult> ObtenerDetalle(Guid facturaGuid, CancellationToken ct)
    {
        var detalles = await _facturas.ListarDetallesAsync(facturaGuid, ct);
        return Ok(ApiResponse<object>.Ok(detalles));
    }

    [HttpGet("{facturaGuid:guid}/pagos")]
    public async Task<IActionResult> ListarPagosDeFactura(Guid facturaGuid, CancellationToken ct)
    {
        var pagos = await _pagos.ListarPorFacturaAsync(facturaGuid, ct);
        return Ok(ApiResponse<object>.Ok(pagos));
    }

    [HttpPatch("{facturaGuid:guid}/anular")]
    [Authorize(Roles = "ADMIN,VENDEDOR")]
    public async Task<IActionResult> Anular(Guid facturaGuid, [FromBody] AnularFacturaDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto?.Motivo))
            return BadRequest(ApiResponse<string>.Error("Motivo de anulación obligatorio."));
        var usuario = User.Identity?.Name ?? "api_user";
        var ok = await _facturas.AnularAsync(facturaGuid, dto.Motivo, usuario, ct);
        if (!ok) return NotFound(ApiResponse<string>.Error("Factura no encontrada o ya anulada."));
        return Ok(ApiResponse<string>.Ok("Factura anulada."));
    }

    [HttpGet("{facturaGuid:guid}")]
    public async Task<IActionResult> Obtener(Guid facturaGuid, CancellationToken ct)
    {
        var data = await _facturas.ObtenerPorGuidAsync(facturaGuid, ct);
        return data is null ? NotFound() : Ok(ApiResponse<FacturaDto>.Ok(data));
    }

    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] Guid? clienteGuid,
        [FromQuery] Guid? sucursalGuid,
        [FromQuery] string? estado,
        CancellationToken ct)
    {
        var data = await _facturas.ListarAsync(clienteGuid, sucursalGuid, estado, ct);
        return Ok(ApiResponse<IReadOnlyList<FacturaDto>>.Ok(data));
    }
}
