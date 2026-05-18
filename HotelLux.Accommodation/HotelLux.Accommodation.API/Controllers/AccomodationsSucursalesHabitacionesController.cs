using Asp.Versioning;
using HotelLux.Accommodation.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelLux.Accommodation.API.Controllers;

/// <summary>Ruta canónica marketplace: GET /api/v1/accomodations/sucursales/{sucursalGuid}/habitaciones (endpoints_publicas.txt).</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/accomodations")]
[AllowAnonymous]
public class AccomodationsSucursalesHabitacionesController : ControllerBase
{
    private readonly PublicHabitacionesListing _listing;

    public AccomodationsSucursalesHabitacionesController(PublicHabitacionesListing listing) =>
        _listing = listing;

    [HttpGet("sucursales/{sucursalGuid:guid}/habitaciones")]
    public async Task<IActionResult> Listar(
        Guid sucursalGuid,
        [FromQuery] Guid? tipo_habitacion_guid,
        [FromQuery] DateTime? fecha_inicio,
        [FromQuery] DateTime? fecha_salida,
        CancellationToken cancellationToken = default)
    {
        if (fecha_inicio.HasValue ^ fecha_salida.HasValue)
            return BadRequest(new
            {
                status = 400,
                error = "Parámetros inválidos",
                details = new[] { "fecha_inicio y fecha_salida deben enviarse juntas o omitirse ambas." },
                timestamp = DateTime.UtcNow
            });

        if (fecha_inicio.HasValue && fecha_salida.HasValue)
        {
            var desde = fecha_inicio.Value.Date;
            var hasta = fecha_salida.Value.Date;
            if (hasta <= desde)
                return BadRequest(new
                {
                    status = 400,
                    error = "Rango de fechas inválido",
                    details = new[] { "fecha_salida debe ser mayor que fecha_inicio." },
                    timestamp = DateTime.UtcNow
                });
        }

        var result = await _listing.ListarAsync(
            sucursalGuid, tipo_habitacion_guid, fecha_inicio, fecha_salida, cancellationToken);
        return Ok(result);
    }
}
