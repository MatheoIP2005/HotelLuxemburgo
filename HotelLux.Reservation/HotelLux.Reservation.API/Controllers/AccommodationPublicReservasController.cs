using Asp.Versioning;
using HotelLux.Reservation.API.Helpers;
using HotelLux.Reservation.Business.DTOs.Reserva;
using HotelLux.Reservation.Business.Interfaces;
using HotelLux.Reservation.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelLux.Reservation.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/accommodations/reservas")]
public class AccommodationPublicReservasController : ControllerBase
{
    private readonly IReservaService _service;
    private readonly IAccommodationClient _accommodation;
    private readonly ILogger<AccommodationPublicReservasController> _logger;

    public AccommodationPublicReservasController(
        IReservaService service,
        IAccommodationClient accommodation,
        ILogger<AccommodationPublicReservasController> logger)
    {
        _service = service;
        _accommodation = accommodation;
        _logger = logger;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> CrearYConfirmar([FromBody] CrearReservaPublicRequest request, CancellationToken ct)
    {
        var dto = await PublicReservaCreateMapper.ToInternalAsync(request, _accommodation, ct);

        dto.CreadoPorUsuario ??= "portal_publico";
        dto.CreadoDesdeIp ??= HttpContext.Connection.RemoteIpAddress?.ToString();
        if (string.IsNullOrWhiteSpace(dto.OrigenCanalReserva))
            dto.OrigenCanalReserva = "PORTAL_PUBLICO";

        ReservaCreateDtoMarketplaceNormalizer.Apply(dto);

        const string usuario = "portal_publico";
        var created = await _service.CrearAsync(dto, ct);
        try
        {
            var confirmed = await _service.ConfirmarAsync(created.ReservaGuid, usuario, ct);
            return StatusCode(201, PublicReservaMapper.ToPublicReserva(confirmed));
        }
        catch (Exception ex)
        {
            try
            {
                await _service.CancelarAsync(
                    created.ReservaGuid,
                    "La confirmación no pudo completarse desde el portal público.",
                    usuario,
                    ct);
            }
            catch (Exception cancelEx)
            {
                _logger.LogError(
                    cancelEx,
                    "Fallo al cancelar reserva {ReservaGuid} tras error en confirmación pública.",
                    created.ReservaGuid);
            }

            _logger.LogWarning(ex, "Confirmación pública fallida para reserva {ReservaGuid}", created.ReservaGuid);
            throw;
        }
    }

    [HttpGet("{reservaGuid:guid}")]
    [Authorize]
    public async Task<IActionResult> ObtenerPublico(Guid reservaGuid, CancellationToken ct)
    {
        var data = await _service.ObtenerPorGuidAsync(reservaGuid, ct);
        return Ok(PublicReservaMapper.ToPublicReserva(data));
    }
}
