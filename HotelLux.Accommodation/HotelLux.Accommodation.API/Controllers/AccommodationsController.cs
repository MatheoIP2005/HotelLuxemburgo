using Asp.Versioning;
using HotelLux.Accommodation.API.Models.Marketplace;
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
    [ProducesResponseType(typeof(AccommodationSearchItemDtoPagedResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string? destino,
        [FromQuery] DateTime? fechaInicio,
        [FromQuery] DateTime? fechaFin,
        [FromQuery] int? num_adultos,
        [FromQuery] int? num_habitaciones,
        [FromQuery] int pagina = 1,
        [FromQuery] int limite = 20,
        CancellationToken cancellationToken = default)
    {
        limite = Math.Clamp(limite, 1, 50);
        pagina = Math.Max(1, pagina);

        if (fechaInicio.HasValue ^ fechaFin.HasValue)
            return BadRequest(new { status = 400, error = "Parámetros inválidos",
                details = new[] { "fechaInicio y fechaFin deben enviarse juntas o omitirse ambas." },
                timestamp = DateTime.UtcNow });

        DateOnly? fechaDesde = null;
        DateOnly? fechaHasta = null;
        if (fechaInicio.HasValue && fechaFin.HasValue)
        {
            var desde = fechaInicio.Value.Date;
            var hasta = fechaFin.Value.Date;
            if (hasta <= desde)
                return BadRequest(new { status = 400, error = "Rango de fechas inválido",
                    details = new[] { "fechaFin debe ser mayor que fechaInicio." },
                    timestamp = DateTime.UtcNow });
            fechaDesde = DateOnly.FromDateTime(desde);
            fechaHasta = DateOnly.FromDateTime(hasta);
        }

        if (num_adultos.HasValue && num_adultos <= 0)
            return BadRequest(new { status = 400, error = "Parámetros inválidos",
                details = new[] { "num_adultos debe ser mayor a cero." },
                timestamp = DateTime.UtcNow });

        if (num_habitaciones.HasValue && num_habitaciones <= 0)
            return BadRequest(new { status = 400, error = "Parámetros inválidos",
                details = new[] { "num_habitaciones debe ser mayor a cero." },
                timestamp = DateTime.UtcNow });

        int noches = (fechaInicio.HasValue && fechaFin.HasValue)
            ? (fechaFin!.Value.Date - fechaInicio!.Value.Date).Days
            : 1;

        var baseQuery = _db.Sucursales.AsNoTracking()
            .Where(x => !x.EsEliminado && x.EstadoSucursal == "ACT");

        if (!string.IsNullOrWhiteSpace(destino))
            baseQuery = baseQuery.Where(x =>
                EF.Functions.ILike(x.Ciudad, $"%{destino}%") ||
                EF.Functions.ILike(x.Ubicacion, $"%{destino}%"));

        var total = await baseQuery.CountAsync(cancellationToken);
        var sucursales = await baseQuery
            .OrderBy(x => x.NombreSucursal)
            .Skip((pagina - 1) * limite)
            .Take(limite)
            .ToListAsync(cancellationToken);

        var resumenesStay = await Task.WhenAll(
            sucursales.Select(s => _stay.GetRatingSummaryAsync(s.SucursalGuid, cancellationToken)));

        var data = new List<AccommodationSearchItemDto>();

        for (var idx = 0; idx < sucursales.Count; idx++)
        {
            var s = sucursales[idx];
            var resumenStay = resumenesStay[idx];
            var unidadesRestantes = await _db.Habitaciones.CountAsync(h =>
                h.IdSucursal == s.IdSucursal &&
                h.EstadoHabitacion == "DIS" &&
                !h.EsEliminado, cancellationToken);

            decimal precioBase = 0m;
            if (fechaDesde.HasValue && fechaHasta.HasValue)
            {
                precioBase = await _db.Tarifas
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
            }

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

            data.Add(new AccommodationSearchItemDto
            {
                SucursalGuid = s.SucursalGuid,
                Nombre = s.NombreSucursal,
                Ciudad = s.Ciudad,
                Provincia = null,
                Pais = s.Pais,
                Direccion = s.Direccion,
                Descripcion = s.DescripcionCorta,
                Categoria = s.CategoriaViaje,
                Estrellas = s.Estrellas ?? 0,
                TipoAlojamiento = s.TipoAlojamiento,
                PrecioDesde = precioBase,
                Moneda = "USD",
                ImagenPrincipalUrl = imagenPrincipal,
                PromedioValoracion = resumenStay != null && resumenStay.TieneResenas
                    ? Math.Round(resumenStay.PromedioGeneral, 2) : 0.0,
                TotalValoraciones = resumenStay?.TotalResenas ?? 0,
                HabitacionesDisponibles = unidadesRestantes,
                ServiciosDestacados = serviciosDestacados,
                HoraCheckIn = s.HoraCheckin,
                HoraCheckOut = s.HoraCheckout,
                AceptaNinos = s.AceptaNinos,
                PermiteMascotas = s.PermiteMascotas
            });
        }

        var totalPaginas = (int)Math.Ceiling(total / (double)limite);
        return Ok(new AccommodationSearchItemDtoPagedResponse
        {
            Items = data,
            Pagina = pagina,
            Limite = limite,
            TotalResultados = total,
            TotalPaginas = totalPaginas,
            TieneSiguiente = pagina < totalPaginas,
            TieneAnterior = pagina > 1
        });
    }

    [AllowAnonymous]
    [HttpGet("categories")]
    [ProducesResponseType(typeof(IReadOnlyList<AccommodationCategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Categories(
        [FromQuery] string? ciudad, [FromQuery] string idioma = "es",
        CancellationToken cancellationToken = default)
    {
        var ciudadNorm = string.IsNullOrWhiteSpace(ciudad) ? "__all__" : ciudad.Trim().ToLowerInvariant();
        var cacheKey = $"accommodations.categories.v1.{ciudadNorm}.{idioma.Trim().ToLowerInvariant()}";
        if (!_cache.TryGetValue(cacheKey, out IReadOnlyList<AccommodationCategoryDto>? result))
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

            result = grupos.Select(x => new AccommodationCategoryDto
            {
                IdCategoria = x.IdCategoria,
                NombreCategoria = x.NombreCategoria,
                TotalPropiedades = x.TotalPropiedades,
                PrecioPromedioNoche = x.PrecioPromedioNoche,
                Moneda = "USD"
            }).ToList();

            _cache.Set(cacheKey, result, TimeSpan.FromHours(6));
        }

        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("{sucursalGuid}")]
    [ProducesResponseType(typeof(AccommodationDetailResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(
        string sucursalGuid,
        [FromQuery] DateTime? fechaInicio = null,
        [FromQuery] DateTime? fechaFin = null,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(sucursalGuid, out var sucursalGuidParsed))
            return BadRequest(new { status = 400, error = "Parámetro inválido",
                details = new[] { "El formato de sucursalGuid no es un UUID válido." },
                timestamp = DateTime.UtcNow });

        var sucursal = await _db.Sucursales.AsNoTracking()
            .FirstOrDefaultAsync(s => s.SucursalGuid == sucursalGuidParsed && !s.EsEliminado && s.EstadoSucursal == "ACT", cancellationToken);

        if (sucursal is null)
            return NotFound(new { status = 404, error = "Recurso no encontrado",
                details = new[] { "Alojamiento no existe o está inactivo." }, timestamp = DateTime.UtcNow });

        if (fechaInicio.HasValue ^ fechaFin.HasValue)
            return BadRequest(new { status = 400, error = "Parámetros inválidos",
                details = new[] { "fechaInicio y fechaFin deben enviarse juntas o omitirse ambas." },
                timestamp = DateTime.UtcNow });

        if (fechaInicio.HasValue && fechaFin.HasValue)
        {
            var desde = fechaInicio.Value.Date;
            var hasta = fechaFin.Value.Date;
            if (hasta <= desde)
                return BadRequest(new { status = 400, error = "Rango de fechas inválido",
                    details = new[] { "fechaFin debe ser mayor que fechaInicio." }, timestamp = DateTime.UtcNow });
        }

        var imagenesSucursal = await _db.SucursalImagenes.AsNoTracking()
            .Where(i => i.IdSucursal == sucursal.IdSucursal)
            .OrderBy(i => i.OrdenVisualizacion)
            .Select(i => i.UrlImagen)
            .ToListAsync(cancellationToken);

        var imagenPrincipal = imagenesSucursal.FirstOrDefault();

        Dictionary<Guid, int>? disponiblesPorTipo = null;
        if (fechaInicio.HasValue && fechaFin.HasValue)
        {
            var disponibles = await _habitaciones.ListarDisponiblesAsync(
                sucursalGuidParsed,
                DateOnly.FromDateTime(fechaInicio.Value.Date),
                DateOnly.FromDateTime(fechaFin.Value.Date),
                cancellationToken);
            disponiblesPorTipo = disponibles
                .GroupBy(h => h.TipoHabitacionGuid)
                .ToDictionary(g => g.Key, g => g.Count());
        }

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
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        var tiposHabitacion = tiposRaw.Select(t => new AccommodationRoomTypeDto
        {
            TipoHabitacionGuid = t.TipoHabitacionGuid,
            Nombre = t.NombreTipoHabitacion,
            TipoCama = t.TipoCama,
            CapacidadAdultos = t.CapacidadAdultos,
            CapacidadNinos = t.CapacidadNinos,
            AreaM2 = t.AreaM2 ?? 0m,
            PrecioBase = t.PrecioBase ?? 0m,
            Imagenes = t.Imagenes,
            DisponiblesEnRango = disponiblesPorTipo is null
                ? 0
                : disponiblesPorTipo.GetValueOrDefault(t.TipoHabitacionGuid, 0)
        }).ToList();

        var tarifasActivas = await _db.Tarifas.AsNoTracking()
            .Where(t => t.IdSucursal == sucursal.IdSucursal
                && t.EstadoTarifa == "ACT" && !t.EsEliminado && t.PermitePortalPublico)
            .Select(t => new AccommodationTariffDto
            {
                TarifaGuid = t.TarifaGuid,
                Nombre = t.NombreTarifa,
                PrecioPorNoche = t.PrecioPorNoche,
                Moneda = "USD",
                FechaInicio = ToUtcDateTimeOffset(t.FechaInicio),
                FechaFin = ToUtcDateTimeOffset(t.FechaFin),
                MinNoches = t.MinNoches,
                TipoHabitacionGuid = t.TipoHabitacion.TipoHabitacionGuid
            })
            .ToListAsync(cancellationToken);

        var amenities = await _db.CatalogoServicios.AsNoTracking()
            .Where(c => c.IdSucursal == sucursal.IdSucursal
                && !c.EsEliminado && c.EstadoCatalogo == "ACT" && c.TipoCatalogo == "AME")
            .Select(c => c.NombreCatalogo)
            .ToListAsync(cancellationToken);

        var resumenDetail = await _stay.GetRatingSummaryAsync(sucursal.SucursalGuid, cancellationToken);

        var precioDesde = tarifasActivas.Count > 0
            ? tarifasActivas.Min(t => t.PrecioPorNoche)
            : (tiposHabitacion.Count > 0 ? tiposHabitacion.Min(t => t.PrecioBase) : 0m);

        var totalDisponibles = disponiblesPorTipo is not null
            ? disponiblesPorTipo.Values.Sum()
            : await _db.Habitaciones.CountAsync(h =>
                h.IdSucursal == sucursal.IdSucursal
                && h.EstadoHabitacion == "DIS"
                && !h.EsEliminado, cancellationToken);

        return Ok(new AccommodationDetailResponse
        {
            SucursalGuid = sucursal.SucursalGuid,
            Nombre = sucursal.NombreSucursal,
            Ciudad = sucursal.Ciudad,
            Provincia = null,
            Pais = sucursal.Pais,
            Direccion = sucursal.Direccion,
            Descripcion = sucursal.DescripcionCorta,
            DescripcionCompleta = sucursal.DescripcionSucursal,
            Categoria = sucursal.CategoriaViaje,
            Estrellas = sucursal.Estrellas ?? 0,
            TipoAlojamiento = sucursal.TipoAlojamiento,
            PrecioDesde = precioDesde,
            Moneda = "USD",
            ImagenPrincipalUrl = imagenPrincipal,
            PromedioValoracion = resumenDetail != null && resumenDetail.TieneResenas
                ? Math.Round(resumenDetail.PromedioGeneral, 2) : 0.0,
            TotalValoraciones = resumenDetail?.TotalResenas ?? 0,
            HabitacionesDisponibles = totalDisponibles,
            ServiciosDestacados = amenities,
            HoraCheckIn = sucursal.HoraCheckin,
            HoraCheckOut = sucursal.HoraCheckout,
            AceptaNinos = sucursal.AceptaNinos,
            PermiteMascotas = sucursal.PermiteMascotas,
            TiposHabitacion = tiposHabitacion,
            TarifasActivas = tarifasActivas,
            Amenities = amenities,
            Imagenes = imagenesSucursal,
            Politicas = new AccommodationPolicyDto
            {
                HoraCheckIn = sucursal.HoraCheckin,
                HoraCheckOut = sucursal.HoraCheckout,
                AceptaNinos = sucursal.AceptaNinos,
                PermiteMascotas = sucursal.PermiteMascotas,
                Politicas = "Según condiciones de la tarifa seleccionada"
            }
        });
    }

    [AllowAnonymous]
    [HttpGet("{sucursalGuid}/reviews")]
    [ProducesResponseType(typeof(AccommodationReviewDtoPagedResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReviews(
        string sucursalGuid, [FromQuery] int pagina = 1, [FromQuery] int limite = 10,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(sucursalGuid, out var sucursalGuidParsed))
            return BadRequest(new { status = 400, error = "Parámetro inválido",
                details = new[] { "El formato de sucursalGuid no es un UUID válido." },
                timestamp = DateTime.UtcNow });

        pagina = Math.Max(1, pagina);
        limite = Math.Clamp(limite, 1, 50);

        var existe = await _db.Sucursales.AnyAsync(
            x => x.SucursalGuid == sucursalGuidParsed && !x.EsEliminado, cancellationToken);

        if (!existe)
            return NotFound(new { status = 404, error = "Recurso no encontrado",
                details = new[] { "Alojamiento no existe." }, timestamp = DateTime.UtcNow });

        var pageResult = await _stay.GetReviewsBySucursalAsync(sucursalGuidParsed, pagina, limite, cancellationToken);
        var totalItems = pageResult?.TotalItems ?? 0;
        var totalPaginas = pageResult?.TotalPages ?? 0;
        var items = pageResult?.Items ?? Array.Empty<StayReviewDto>();

        var items2 = items.Select(r => new AccommodationReviewDto
        {
            ValoracionGuid = r.ValoracionGuid,
            Puntuacion = (int)Math.Round(r.PuntuacionGeneral),
            ComentarioPositivo = r.ComentarioPositivo,
            ComentarioNegativo = r.ComentarioNegativo,
            TipoViaje = r.TipoViaje,
            Fecha = DateTimeOffset.TryParse(r.FechaPublicacion, out var fechaPub)
                ? fechaPub
                : DateTimeOffset.UtcNow,
            NombreVisibleCliente = r.NombreVisibleCliente,
            RespuestaPropiedad = r.RespuestaHotel
        }).ToList();

        return Ok(new AccommodationReviewDtoPagedResponse
        {
            Items = items2,
            Pagina = pagina,
            Limite = limite,
            TotalResultados = totalItems,
            TotalPaginas = totalPaginas,
            TieneSiguiente = totalPaginas > 0 && pagina < totalPaginas,
            TieneAnterior = pagina > 1
        });
    }

    private static DateTimeOffset ToUtcDateTimeOffset(DateOnly date) =>
        new(date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
}
