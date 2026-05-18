using Asp.Versioning;
using HotelLux.Accommodation.API.Services;
using HotelLux.Accommodation.Business.DTOs.Habitacion;
using HotelLux.Accommodation.Business.Interfaces;
using HotelLux.Accommodation.DataAccess.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace HotelLux.Accommodation.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/accommodations")]
public class AccommodationsController : ControllerBase
{
    private readonly AccommodationDbContext _db;
    private readonly IMemoryCache _cache;
    private readonly IHabitacionService _habitaciones;
    private readonly IStayPublicClient _stay;

    public AccommodationsController(
        AccommodationDbContext db,
        IMemoryCache cache,
        IHabitacionService habitaciones,
        IStayPublicClient stay)
    {
        _db = db;
        _cache = cache;
        _habitaciones = habitaciones;
        _stay = stay;
    }

    [AllowAnonymous]
    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? destino,
        [FromQuery] DateTime? fecha_entrada,
        [FromQuery] DateTime? fecha_salida,
        [FromQuery] int? num_adultos,
        [FromQuery] int? num_habitaciones,
        [FromQuery] int pagina = 1,
        [FromQuery] int limite = 20,
        CancellationToken cancellationToken = default)
    {
        limite = Math.Clamp(limite, 1, 50);
        pagina = Math.Max(1, pagina);

        if (string.IsNullOrWhiteSpace(destino) || !fecha_entrada.HasValue || !fecha_salida.HasValue
            || !num_adultos.HasValue || !num_habitaciones.HasValue)
        {
            return BadRequest(new { status = 400, error = "Parámetros inválidos",
                details = new[] { "destino, fecha_entrada, fecha_salida, num_adultos y num_habitaciones son requeridos." },
                timestamp = DateTime.UtcNow });
        }

        if (num_adultos <= 0 || num_habitaciones <= 0)
            return BadRequest(new { status = 400, error = "Parámetros inválidos",
                details = new[] { "num_adultos y num_habitaciones deben ser mayores a cero." },
                timestamp = DateTime.UtcNow });

        var desde = fecha_entrada.Value.Date;
        var hasta = fecha_salida.Value.Date;
        if (hasta <= desde)
            return BadRequest(new { status = 400, error = "Rango de fechas inválido",
                details = new[] { "fecha_salida debe ser mayor que fecha_entrada." }, timestamp = DateTime.UtcNow });

        var noches = (hasta - desde).Days;

        var baseQuery = _db.Sucursales.AsNoTracking()
            .Where(x => !x.EsEliminado && x.EstadoSucursal == "ACT"
                && (EF.Functions.ILike(x.Ciudad, $"%{destino}%")
                    || EF.Functions.ILike(x.Ubicacion, $"%{destino}%")));

        var total = await baseQuery.CountAsync(cancellationToken);
        var sucursales = await baseQuery
            .OrderBy(x => x.NombreSucursal)
            .Skip((pagina - 1) * limite)
            .Take(limite)
            .ToListAsync(cancellationToken);

        var resumenesStay = await Task.WhenAll(
            sucursales.Select(s => _stay.GetRatingSummaryAsync(s.SucursalGuid, cancellationToken)));

        var data = new List<object>();
        var fechaDesde = DateOnly.FromDateTime(desde);
        var fechaHasta = DateOnly.FromDateTime(hasta);

        for (var idx = 0; idx < sucursales.Count; idx++)
        {
            var s = sucursales[idx];
            var resumenStay = resumenesStay[idx];
            var unidadesRestantes = await _db.Habitaciones.CountAsync(h =>
                h.IdSucursal == s.IdSucursal &&
                h.EstadoHabitacion == "DIS" &&
                !h.EsEliminado, cancellationToken);

            var precioBase = await _db.Tarifas
                .Where(t => t.IdSucursal == s.IdSucursal
                    && t.FechaInicio <= fechaDesde
                    && t.FechaFin >= fechaHasta
                    && t.EstadoTarifa == "ACT"
                    && !t.EsEliminado
                    && t.PermitePortalPublico
                    && noches >= t.MinNoches
                    && noches <= (t.MaxNoches ?? 99999))
                .OrderBy(t => t.Prioridad)
                .Select(t => (decimal?)t.PrecioPorNoche)
                .FirstOrDefaultAsync(cancellationToken) ?? 0m;

            if (precioBase <= 0m)
                precioBase = await _db.Habitaciones
                    .Where(h => h.IdSucursal == s.IdSucursal && !h.EsEliminado)
                    .OrderBy(h => h.PrecioBase)
                    .Select(h => (decimal?)h.PrecioBase)
                    .FirstOrDefaultAsync(cancellationToken) ?? 0m;

            string? imagenPrincipal = await (
                from h in _db.Habitaciones
                join i in _db.TipoHabitacionImagenes on h.IdTipoHabitacion equals i.IdTipoHabitacion
                where h.IdSucursal == s.IdSucursal && i.EsPrincipal
                orderby i.OrdenVisualizacion
                select i.UrlImagen
            ).FirstOrDefaultAsync(cancellationToken);

            if (imagenPrincipal is null)
                imagenPrincipal = await _db.SucursalImagenes
                    .Where(i => i.IdSucursal == s.IdSucursal && i.EsPrincipal)
                    .OrderBy(i => i.OrdenVisualizacion)
                    .Select(i => i.UrlImagen)
                    .FirstOrDefaultAsync(cancellationToken);

            var serviciosDestacados = await _db.CatalogoServicios.AsNoTracking()
                .Where(c => c.IdSucursal == s.IdSucursal && !c.EsEliminado && c.EstadoCatalogo == "ACT")
                .OrderBy(c => c.NombreCatalogo)
                .Take(20)
                .Select(c => c.NombreCatalogo)
                .ToListAsync(cancellationToken);

            data.Add(new
            {
                sucursalGuid = s.SucursalGuid,
                nombre = s.NombreSucursal,
                ciudad = s.Ciudad,
                provincia = (string?)null,
                pais = s.Pais,
                direccion = s.Direccion,
                descripcion = s.DescripcionCorta,
                categoria = s.CategoriaViaje,
                estrellas = s.Estrellas,
                tipoAlojamiento = s.TipoAlojamiento,
                precioDesde = precioBase,
                moneda = "USD",
                imagenPrincipalUrl = imagenPrincipal,
                promedioValoracion = resumenStay != null && resumenStay.TieneResenas
                    ? Math.Round(resumenStay.PromedioGeneral, 2) : 0.0,
                totalValoraciones = resumenStay?.TotalResenas ?? 0,
                habitacionesDisponibles = unidadesRestantes,
                serviciosDestacados,
                horaCheckIn = s.HoraCheckin,
                horaCheckOut = s.HoraCheckout,
                aceptaNinos = s.AceptaNinos,
                permiteMascotas = s.PermiteMascotas
            });
        }

        var totalPaginas = (int)Math.Ceiling(total / (double)limite);
        return Ok(new
        {
            items = data,
            pagina,
            limite,
            totalResultados = total,
            totalPaginas,
            tieneSiguiente = pagina < totalPaginas,
            tieneAnterior = pagina > 1
        });
    }

    [AllowAnonymous]
    [HttpGet("categories")]
    public async Task<IActionResult> Categories(
        [FromQuery] string? ciudad, [FromQuery] string idioma = "es",
        CancellationToken cancellationToken = default)
    {
        var ciudadNorm = string.IsNullOrWhiteSpace(ciudad) ? "__all__" : ciudad.Trim().ToLowerInvariant();
        var cacheKey = $"accommodations.categories.v1.{ciudadNorm}.{idioma.Trim().ToLowerInvariant()}";
        if (!_cache.TryGetValue(cacheKey, out object? result))
        {
            var baseQ = _db.Sucursales.AsNoTracking()
                .Where(s => !s.EsEliminado && s.EstadoSucursal == "ACT" && s.CategoriaViaje != null);
            if (!string.IsNullOrWhiteSpace(ciudad))
                baseQ = baseQ.Where(s => s.Ciudad == ciudad!.Trim());

            var grupos = await baseQ
                .GroupBy(s => s.CategoriaViaje!)
                .Select(g => new
                {
                    IdCategoria = g.Key,
                    NombreCategoria = g.Key,
                    TotalPropiedades = g.Count(),
                    PrecioPromedioNoche = _db.Habitaciones
                        .Where(h => g.Select(x => x.IdSucursal).Contains(h.IdSucursal)
                            && !h.EsEliminado && h.EstadoHabitacion == "DIS")
                        .Average(h => (decimal?)h.PrecioBase) ?? 0m
                })
                .ToListAsync(cancellationToken);

            result = grupos.Select(x => new
            {
                idCategoria = x.IdCategoria,
                nombreCategoria = x.NombreCategoria,
                totalPropiedades = x.TotalPropiedades,
                precioPromedioNoche = x.PrecioPromedioNoche,
                moneda = "USD"
            }).ToList();

            _cache.Set(cacheKey, result, TimeSpan.FromHours(6));
        }

        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("{sucursalGuid:guid}")]
    public async Task<IActionResult> GetById(
        Guid sucursalGuid,
        [FromQuery] DateTime? fecha_entrada = null,
        [FromQuery] DateTime? fecha_salida = null,
        CancellationToken cancellationToken = default)
    {
        var sucursal = await _db.Sucursales.AsNoTracking()
            .FirstOrDefaultAsync(s => s.SucursalGuid == sucursalGuid && !s.EsEliminado && s.EstadoSucursal == "ACT", cancellationToken);

        if (sucursal is null)
            return NotFound(new { status = 404, error = "Recurso no encontrado",
                details = new[] { "Alojamiento no existe o está inactivo." }, timestamp = DateTime.UtcNow });

        if (fecha_entrada.HasValue && fecha_salida.HasValue)
        {
            var desde = fecha_entrada.Value.Date;
            var hasta = fecha_salida.Value.Date;
            if (hasta <= desde)
                return BadRequest(new { status = 400, error = "Rango de fechas inválido",
                    details = new[] { "fecha_salida debe ser mayor que fecha_entrada." }, timestamp = DateTime.UtcNow });
        }

        var imagenesSucursal = await _db.SucursalImagenes.AsNoTracking()
            .Where(i => i.IdSucursal == sucursal.IdSucursal)
            .OrderBy(i => i.OrdenVisualizacion)
            .Select(i => i.UrlImagen)
            .ToListAsync(cancellationToken);

        var imagenPrincipal = imagenesSucursal.FirstOrDefault();

        var tiposRaw = await _db.TiposHabitacion.AsNoTracking()
            .Where(t => !t.EsEliminado && t.EstadoTipoHabitacion == "ACT"
                && t.Habitaciones.Any(h => h.IdSucursal == sucursal.IdSucursal && !h.EsEliminado))
            .Select(t => new
            {
                t.TipoHabitacionGuid,
                t.NombreTipoHabitacion,
                t.TipoCama,
                t.CapacidadAdultos,
                t.CapacidadNinos,
                t.AreaM2,
                PrecioBase = t.Habitaciones
                    .Where(h => h.IdSucursal == sucursal.IdSucursal && !h.EsEliminado)
                    .OrderBy(h => h.PrecioBase)
                    .Select(h => (decimal?)h.PrecioBase)
                    .FirstOrDefault(),
                Imagenes = t.Imagenes
                    .OrderBy(i => i.OrdenVisualizacion)
                    .Select(i => i.UrlImagen)
                    .ToList(),
                DisponiblesEnRango = t.Habitaciones
                    .Count(h => h.IdSucursal == sucursal.IdSucursal
                        && h.EstadoHabitacion == "DIS" && !h.EsEliminado)
            })
            .ToListAsync(cancellationToken);

        var tiposHabitacion = tiposRaw.Select(t => new
        {
            tipoHabitacionGuid = t.TipoHabitacionGuid,
            nombre = t.NombreTipoHabitacion,
            tipoCama = t.TipoCama,
            capacidadAdultos = t.CapacidadAdultos,
            capacidadNinos = t.CapacidadNinos,
            areaM2 = t.AreaM2,
            precioBase = t.PrecioBase ?? 0m,
            imagenes = t.Imagenes,
            disponiblesEnRango = t.DisponiblesEnRango
        }).ToList();

        var tarifasActivas = await _db.Tarifas.AsNoTracking()
            .Where(t => t.IdSucursal == sucursal.IdSucursal
                && t.EstadoTarifa == "ACT" && !t.EsEliminado && t.PermitePortalPublico)
            .Select(t => new
            {
                tarifaGuid = t.TarifaGuid,
                nombre = t.NombreTarifa,
                precioPorNoche = t.PrecioPorNoche,
                moneda = "USD",
                fechaInicio = t.FechaInicio,
                fechaFin = t.FechaFin,
                minNoches = t.MinNoches,
                tipoHabitacionGuid = t.TipoHabitacion.TipoHabitacionGuid
            })
            .ToListAsync(cancellationToken);

        var amenities = await _db.CatalogoServicios.AsNoTracking()
            .Where(c => c.IdSucursal == sucursal.IdSucursal
                && !c.EsEliminado && c.EstadoCatalogo == "ACT" && c.TipoCatalogo == "AME")
            .Select(c => c.NombreCatalogo)
            .ToListAsync(cancellationToken);

        var resumenDetail = await _stay.GetRatingSummaryAsync(sucursal.SucursalGuid, cancellationToken);

        var precioDesde = tarifasActivas.Count > 0
            ? tarifasActivas.Min(t => t.precioPorNoche)
            : (tiposHabitacion.Count > 0 ? tiposHabitacion.Min(t => t.precioBase) : 0m);

        var totalDisponibles = tiposRaw.Sum(t => t.DisponiblesEnRango);

        var porTipoHabitacion = (fecha_entrada.HasValue && fecha_salida.HasValue)
            ? tiposRaw.Select(t => (object)new
            {
                tipoHabitacionGuid = t.TipoHabitacionGuid,
                nombre = t.NombreTipoHabitacion,
                disponibles = t.DisponiblesEnRango
            }).ToList()
            : new List<object>();

        return Ok(new
        {
            sucursalGuid = sucursal.SucursalGuid,
            nombre = sucursal.NombreSucursal,
            ciudad = sucursal.Ciudad,
            provincia = (string?)null,
            pais = sucursal.Pais,
            direccion = sucursal.Direccion,
            descripcion = sucursal.DescripcionCorta,
            descripcionCompleta = sucursal.DescripcionSucursal,
            categoria = sucursal.CategoriaViaje,
            estrellas = sucursal.Estrellas,
            tipoAlojamiento = sucursal.TipoAlojamiento,
            precioDesde,
            moneda = "USD",
            imagenPrincipalUrl = imagenPrincipal,
            promedioValoracion = resumenDetail != null && resumenDetail.TieneResenas
                ? Math.Round(resumenDetail.PromedioGeneral, 2) : 0.0,
            totalValoraciones = resumenDetail?.TotalResenas ?? 0,
            habitacionesDisponibles = totalDisponibles,
            serviciosDestacados = amenities,
            horaCheckIn = sucursal.HoraCheckin,
            horaCheckOut = sucursal.HoraCheckout,
            aceptaNinos = sucursal.AceptaNinos,
            permiteMascotas = sucursal.PermiteMascotas,
            tiposHabitacion,
            tarifasActivas,
            amenities,
            imagenes = imagenesSucursal,
            politicas = new
            {
                horaCheckIn = sucursal.HoraCheckin,
                horaCheckOut = sucursal.HoraCheckout,
                aceptaNinos = sucursal.AceptaNinos,
                permiteMascotas = sucursal.PermiteMascotas,
                politicas = "Según condiciones de la tarifa seleccionada"
            },
            disponibilidad = new
            {
                fechaEntrada = fecha_entrada,
                fechaSalida = fecha_salida,
                porTipoHabitacion
            }
        });
    }

    [AllowAnonymous]
    [HttpGet("{sucursalGuid:guid}/reviews")]
    public async Task<IActionResult> GetReviews(
        Guid sucursalGuid, [FromQuery] int pagina = 1, [FromQuery] int limite = 10,
        CancellationToken cancellationToken = default)
    {
        pagina = Math.Max(1, pagina);
        limite = Math.Clamp(limite, 1, 50);

        var existe = await _db.Sucursales.AnyAsync(
            x => x.SucursalGuid == sucursalGuid && !x.EsEliminado, cancellationToken);

        if (!existe)
            return NotFound(new { status = 404, error = "Recurso no encontrado",
                details = new[] { "Alojamiento no existe." }, timestamp = DateTime.UtcNow });

        var pageResult = await _stay.GetReviewsBySucursalAsync(sucursalGuid, pagina, limite, cancellationToken);
        var totalItems = pageResult?.TotalItems ?? 0;
        var totalPaginas = pageResult?.TotalPages ?? 0;
        var items = pageResult?.Items ?? Array.Empty<StayReviewDto>();

        var items2 = items.Select(r => new
        {
            valoracionGuid = r.ValoracionGuid,
            puntuacion = r.PuntuacionGeneral,
            comentarioPositivo = r.ComentarioPositivo,
            comentarioNegativo = r.ComentarioNegativo,
            tipoViaje = r.TipoViaje,
            fecha = r.FechaPublicacion,
            nombreVisibleCliente = r.NombreVisibleCliente,
            respuestaPropiedad = r.RespuestaHotel
        }).ToList();

        return Ok(new
        {
            items = items2,
            pagina,
            limite,
            totalResultados = totalItems,
            totalPaginas,
            tieneSiguiente = totalPaginas > 0 && pagina < totalPaginas,
            tieneAnterior = pagina > 1
        });
    }
}
