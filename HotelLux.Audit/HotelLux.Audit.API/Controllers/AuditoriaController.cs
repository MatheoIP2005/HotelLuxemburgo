using Asp.Versioning;
using HotelLux.Audit.API.Models;
using HotelLux.Audit.API.Models.Common;
using HotelLux.Audit.DataAccess.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelLux.Audit.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/internal/auditoria")]
[Authorize(Roles = "ADMINISTRADOR")]
public class AuditoriaController : ControllerBase
{
    private readonly IEventoAuditoriaRepository _repo;
    public AuditoriaController(IEventoAuditoriaRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] string? servicioOrigen,
        [FromQuery] string? tablaAfectada,
        [FromQuery] Guid? entidadGuid,
        [FromQuery] string? usuarioEjecutor,
        [FromQuery] int pagina = 1,
        [FromQuery] int limite = 50,
        CancellationToken ct = default)
    {
        pagina = pagina < 1 ? 1 : pagina;
        limite = limite < 1 ? 50 : Math.Min(limite, 200);
        var data = await _repo.ListarAsync(
            servicioOrigen, tablaAfectada, entidadGuid, usuarioEjecutor, pagina, limite, ct);
        return Ok(ApiResponse<object>.Ok(new { items = data, pagina, limite }));
    }

    [HttpGet("{auditoriaGuid:guid}")]
    public async Task<IActionResult> ObtenerPorGuid(Guid auditoriaGuid, CancellationToken ct = default)
    {
        var e = await _repo.ObtenerPorAuditoriaGuidAsync(auditoriaGuid, ct);
        if (e is null)
            return NotFound(ApiResponse<object>.Fail("Evento de auditoría no encontrado."));
        var dto = new AuditoriaEventoDetalleDto
        {
            AuditoriaGuid = e.AuditoriaGuid,
            TablaAfectada = e.TablaAfectada,
            Operacion = e.Operacion,
            EntidadGuid = e.EntidadGuid,
            IdRegistroAfectado = e.IdRegistroAfectado,
            DatosAnteriores = e.DatosAnteriores,
            DatosNuevos = e.DatosNuevos,
            UsuarioEjecutor = e.UsuarioEjecutor,
            UsuarioGuid = e.UsuarioGuid,
            IpOrigen = e.IpOrigen,
            ServicioOrigen = e.ServicioOrigen,
            FechaEventoUtc = e.FechaEventoUtc
        };
        return Ok(ApiResponse<AuditoriaEventoDetalleDto>.Ok(dto));
    }
}
