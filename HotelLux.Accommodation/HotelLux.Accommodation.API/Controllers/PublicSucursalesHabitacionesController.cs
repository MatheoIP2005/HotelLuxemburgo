using Asp.Versioning;
using HotelLux.Accommodation.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelLux.Accommodation.API.Controllers;

/// <summary>Spec: GET /api/v1/public/sucursales/{sucursalGuid}/habitaciones</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/public/sucursales/{sucursalGuid:guid}/habitaciones")]
[AllowAnonymous]
public class PublicSucursalesHabitacionesController : ControllerBase
{
    private readonly PublicHabitacionesListing _listing;

    public PublicSucursalesHabitacionesController(PublicHabitacionesListing listing) =>
        _listing = listing;

    [HttpGet]
    public async Task<IActionResult> Listar(
        Guid sucursalGuid,
        [FromQuery] Guid? tipo_habitacion_guid,
        [FromQuery] DateTime? fechaInicio,
        [FromQuery] DateTime? fechaFin,
        CancellationToken cancellationToken = default)
    {
        if (fechaInicio.HasValue ^ fechaFin.HasValue)
            return BadRequest(new
            {
                status = 400,
                error = "Parámetros inválidos",
                details = new[] { "fechaInicio y fechaFin deben enviarse juntas o omitirse ambas." },
                timestamp = DateTime.UtcNow
            });

        if (fechaInicio.HasValue && fechaFin.HasValue)
        {
            var desde = fechaInicio.Value.Date;
            var hasta = fechaFin.Value.Date;
            if (hasta <= desde)
                return BadRequest(new
                {
                    status = 400,
                    error = "Rango de fechas inválido",
                    details = new[] { "fechaFin debe ser mayor que fechaInicio." },
                    timestamp = DateTime.UtcNow
                });
        }

        var result = await _listing.ListarAsync(
            sucursalGuid, tipo_habitacion_guid, fechaInicio, fechaFin, cancellationToken);
        return Ok(result);
    }
}
