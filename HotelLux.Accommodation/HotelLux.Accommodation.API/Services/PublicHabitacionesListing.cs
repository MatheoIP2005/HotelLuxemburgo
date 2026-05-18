using HotelLux.Accommodation.API.Models.Public;
using HotelLux.Accommodation.Business.DTOs.Habitacion;
using HotelLux.Accommodation.DataAccess.Context;
using HotelLux.Accommodation.Business.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelLux.Accommodation.API.Services;

public sealed class PublicHabitacionesListing
{
    private readonly AccommodationDbContext _db;
    private readonly IHabitacionService _habitaciones;

    public PublicHabitacionesListing(AccommodationDbContext db, IHabitacionService habitaciones)
    {
        _db = db;
        _habitaciones = habitaciones;
    }

    public async Task<IReadOnlyList<HabitacionPublicListItemResponse>> ListarAsync(
        Guid sucursalGuid,
        Guid? tipoHabitacionGuid,
        DateTime? fechaInicio,
        DateTime? fechaSalida,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<HabitacionDTO> habitaciones;
        if (fechaInicio.HasValue && fechaSalida.HasValue)
        {
            var desde = fechaInicio.Value.Date;
            var hasta = fechaSalida.Value.Date;
            habitaciones = await _habitaciones.ListarDisponiblesAsync(
                sucursalGuid,
                DateOnly.FromDateTime(desde),
                DateOnly.FromDateTime(hasta),
                cancellationToken);
        }
        else
            habitaciones = await _habitaciones.ListarPorSucursalAsync(sucursalGuid, cancellationToken);

        if (tipoHabitacionGuid.HasValue)
            habitaciones = habitaciones.Where(h => h.TipoHabitacionGuid == tipoHabitacionGuid.Value).ToList();

        var tipoIds = habitaciones.Select(h => h.TipoHabitacionGuid).Distinct().ToList();
        var tipoInfo = await _db.TiposHabitacion.AsNoTracking()
            .Where(t => !t.EsEliminado && tipoIds.Contains(t.TipoHabitacionGuid))
            .Select(t => new
            {
                t.TipoHabitacionGuid,
                t.NombreTipoHabitacion,
                t.CapacidadAdultos,
                t.CapacidadNinos
            })
            .ToListAsync(cancellationToken);
        var tipoPorGuid = tipoInfo.ToDictionary(x => x.TipoHabitacionGuid);

        var enRango = fechaInicio.HasValue && fechaSalida.HasValue;
        return habitaciones.Select(h =>
        {
            tipoPorGuid.TryGetValue(h.TipoHabitacionGuid, out var ti);
            var disponibleEnRango = enRango
                ? string.Equals(h.EstadoHabitacion, "DIS", StringComparison.Ordinal)
                : string.Equals(h.EstadoHabitacion, "DIS", StringComparison.Ordinal);
            return new HabitacionPublicListItemResponse
            {
                HabitacionGuid = h.HabitacionGuid,
                TipoHabitacionGuid = h.TipoHabitacionGuid,
                TipoNombre = ti?.NombreTipoHabitacion,
                NumeroHabitacion = h.NumeroHabitacion,
                Piso = h.Piso ?? 0,
                CapacidadAdultos = ti?.CapacidadAdultos ?? 0,
                CapacidadNinos = ti?.CapacidadNinos ?? 0,
                PrecioBase = h.PrecioBase,
                Moneda = "USD",
                EstadoHabitacion = h.EstadoHabitacion,
                DisponibleEnRango = disponibleEnRango
            };
        }).ToList();
    }
}
