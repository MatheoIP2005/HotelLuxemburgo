using System.Text.Json;
using HotelLux.Stay.Business.DTOs;
using HotelLux.Stay.Business.Exceptions;
using HotelLux.Stay.Business.Interfaces;
using HotelLux.Stay.DataManagement.Interfaces;
using HotelLux.Stay.DataManagement.Models;

namespace HotelLux.Stay.Business.Services;

public class ValoracionService : IValoracionService
{
    private readonly IValoracionDataService _valoracionData;
    private readonly IEstadiaDataService    _estadiaData;
    private readonly IAuditEmitter          _audit;

    public ValoracionService(
        IValoracionDataService valoracionData,
        IEstadiaDataService estadiaData,
        IAuditEmitter audit)
    {
        _valoracionData = valoracionData;
        _estadiaData    = estadiaData;
        _audit          = audit;
    }

    public async Task<ValoracionDto> CrearAsync(ValoracionCreateDto dto, CancellationToken ct = default)
    {
        if (dto.EstadiaGuid == Guid.Empty)
            throw new ValidationException("EstadiaGuid es obligatorio.", new[] { "EstadiaGuid requerido." });

        var estadia = await _estadiaData.ObtenerPorGuidAsync(dto.EstadiaGuid, ct);
        if (estadia is null)
            throw new NotFoundException("Estadía", dto.EstadiaGuid);

        if (estadia.Estado != "FIN")
            throw new ConflictException("Valoración",
                "Solo se pueden valorar estadías finalizadas (check-out completado).");

        var usuario = dto.CreadoPorUsuario ?? "stay_api";

        var model = new ValoracionDataModel
        {
            EstadiaGuid             = estadia.EstadiaGuid,
            SucursalGuid            = estadia.SucursalGuid,
            ClienteGuid             = estadia.ClienteGuid,
            PuntuacionGeneral       = dto.PuntuacionGeneral,
            PuntuacionLimpieza      = dto.PuntuacionLimpieza,
            PuntuacionConfort       = dto.PuntuacionConfort,
            PuntuacionUbicacion     = dto.PuntuacionUbicacion,
            PuntuacionInstalaciones = dto.PuntuacionInstalaciones,
            PuntuacionPersonal      = dto.PuntuacionPersonal,
            PuntuacionCalidadPrecio = dto.PuntuacionCalidadPrecio,
            ComentarioPositivo      = dto.ComentarioPositivo.Trim(),
            ComentarioNegativo      = dto.ComentarioNegativo.Trim(),
            TipoViaje               = dto.TipoViaje.Trim().ToUpperInvariant(),
            FechaPublicacionUtc     = DateTimeOffset.UtcNow,
            NombreVisibleCliente    = string.IsNullOrWhiteSpace(dto.NombreVisibleCliente)
                ? null
                : dto.NombreVisibleCliente.Trim(),
            CreadoPorUsuario        = usuario
        };

        var created = await _valoracionData.CrearAsync(model, ct);

        _audit.EmitFireAndForget(
            "stay-service", "estadias.valoracion", "INSERT",
            created.ValoracionGuid.ToString(), created.IdValoracion.ToString(),
            Guid.Empty.ToString(),
            usuario, null,
            null,
            JsonSerializer.Serialize(new
            {
                estadia_guid    = created.EstadiaGuid,
                sucursal_guid   = created.SucursalGuid,
                puntuacion      = created.PuntuacionGeneral,
                tipo_viaje      = created.TipoViaje
            }));

        return ToDto(created);
    }

    public async Task<IReadOnlyList<ValoracionDto>> ListarPorEstadiaAsync(Guid estadiaGuid, CancellationToken ct = default)
    {
        var estadia = await _estadiaData.ObtenerPorGuidAsync(estadiaGuid, ct);
        if (estadia is null)
            throw new NotFoundException("Estadía", estadiaGuid);

        var list = await _valoracionData.ListarPorClienteAsync(estadia.ClienteGuid, ct);
        return list
            .Where(v => v.EstadiaGuid == estadiaGuid)
            .Select(ToDto)
            .ToList();
    }

    public async Task<(IReadOnlyList<ValoracionDto> Items, int Total)> ListarPaginadoAsync(
        int pagina, int limite, CancellationToken ct = default)
    {
        var (items, total) = await _valoracionData.ListarPaginadoAsync(pagina, limite, ct);
        return (items.Select(ToDto).ToList(), total);
    }

    public async Task<ValoracionDto?> ObtenerPorGuidAsync(Guid valoracionGuid, CancellationToken ct = default)
    {
        var m = await _valoracionData.ObtenerPorGuidAsync(valoracionGuid, ct);
        return m is null ? null : ToDto(m);
    }

    public async Task ResponderAsync(Guid valoracionGuid, string respuesta, string usuario, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(respuesta))
            throw new ValidationException("Respuesta obligatoria.", new[] { "Respuesta requerida." });

        await _valoracionData.ActualizarRespuestaAsync(valoracionGuid, respuesta, usuario, ct);

        _audit.EmitFireAndForget(
            "stay-service", "estadias.valoracion", "UPDATE",
            valoracionGuid.ToString(), "0",
            Guid.Empty.ToString(),
            usuario, null,
            null,
            JsonSerializer.Serialize(new { accion = "respuesta_hotel" }));
    }

    public async Task ModerarOcultarAsync(Guid valoracionGuid, string usuario, CancellationToken ct = default)
    {
        await _valoracionData.MarcarOcultaModeracionAsync(valoracionGuid, usuario, ct);

        _audit.EmitFireAndForget(
            "stay-service", "estadias.valoracion", "UPDATE",
            valoracionGuid.ToString(), "0",
            Guid.Empty.ToString(),
            usuario, null,
            null,
            JsonSerializer.Serialize(new { accion = "moderacion_ocultar" }));
    }

    public async Task EliminarAsync(Guid valoracionGuid, string usuario, CancellationToken ct = default)
    {
        await _valoracionData.MarcarOcultaModeracionAsync(valoracionGuid, usuario, ct);

        _audit.EmitFireAndForget(
            "stay-service", "estadias.valoracion", "UPDATE",
            valoracionGuid.ToString(), "0",
            Guid.Empty.ToString(),
            usuario, null,
            null,
            JsonSerializer.Serialize(new { accion = "eliminacion_logica" }));
    }

    private static ValoracionDto ToDto(ValoracionDataModel m) => new()
    {
        ValoracionGuid      = m.ValoracionGuid,
        EstadiaGuid         = m.EstadiaGuid,
        SucursalGuid        = m.SucursalGuid,
        ClienteGuid         = m.ClienteGuid,
        PuntuacionGeneral   = m.PuntuacionGeneral,
        ComentarioPositivo  = m.ComentarioPositivo,
        ComentarioNegativo  = m.ComentarioNegativo,
        TipoViaje           = m.TipoViaje,
        FechaPublicacionUtc = m.FechaPublicacionUtc,
        RespuestaHotel      = m.RespuestaHotel,
        NombreVisibleCliente = m.NombreVisibleCliente
    };
}
