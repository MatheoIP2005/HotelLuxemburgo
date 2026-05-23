using HotelLux.Reservation.Business.DTOs.Reserva;
using HotelLux.Reservation.Business.DTOs.ReservaHabitacion;
using HotelLux.Reservation.Business.Exceptions;
using HotelLux.Reservation.Business.Interfaces;

namespace HotelLux.Reservation.Business.Services;

public static class PublicReservaCreateMapper
{
    public static async Task<ReservaCreateDTO> ToInternalAsync(
        CrearReservaPublicRequest request,
        IAccommodationClient accommodation,
        CancellationToken ct = default)
    {
        if (request.SucursalGuid == Guid.Empty)
            throw new ValidationException("Datos de reserva inválidos.", ["SucursalGuid es obligatorio."]);

        var fechaInicio = DateOnly.FromDateTime(request.FechaInicio.UtcDateTime);
        var fechaFin = DateOnly.FromDateTime(request.FechaFin.UtcDateTime);

        if (fechaFin <= fechaInicio)
            throw new ValidationException("Datos de reserva inválidos.", ["FechaFin debe ser posterior a FechaInicio."]);

        if (request.Habitaciones is null || request.Habitaciones.Count == 0)
            throw new ValidationException("Datos de reserva inválidos.", ["La reserva debe incluir al menos una habitación."]);

        if (request.Cliente is null)
            throw new ValidationException("Datos de reserva inválidos.", ["Se requiere un objeto cliente."]);

        var asignadas = new HashSet<Guid>();
        var lineas = new List<ReservaHabitacionCreateDTO>();

        foreach (var pub in request.Habitaciones)
        {
            if (pub.TipoHabitacionGuid == Guid.Empty)
                throw new ValidationException("Datos de reserva inválidos.",
                    ["Cada habitación debe incluir tipoHabitacionGuid."]);

            var cantidad = pub.NumHabitaciones <= 0 ? 1 : pub.NumHabitaciones;
            var adultos = pub.NumAdultos <= 0 ? 1 : pub.NumAdultos;

            var disponibles = await accommodation.ListarDisponiblesAsync(
                request.SucursalGuid,
                fechaInicio,
                fechaFin,
                pub.TipoHabitacionGuid,
                adultos,
                ct);

            var elegidas = disponibles
                .Where(h => !asignadas.Contains(h.HabitacionGuid))
                .Take(cantidad)
                .ToList();

            if (elegidas.Count < cantidad)
            {
                throw new ValidationException(
                    "No hay disponibilidad suficiente para el tipo de habitación solicitado.",
                    [$"Tipo {pub.TipoHabitacionGuid}: se requieren {cantidad}, disponibles {elegidas.Count}."]);
            }

            foreach (var hab in elegidas)
            {
                asignadas.Add(hab.HabitacionGuid);
                lineas.Add(new ReservaHabitacionCreateDTO
                {
                    HabitacionGuid = hab.HabitacionGuid,
                    TipoHabitacionGuid = pub.TipoHabitacionGuid,
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin,
                    NumAdultos = adultos,
                    NumNinos = pub.NumNinos < 0 ? 0 : pub.NumNinos,
                    PrecioNocheAplicado = hab.PrecioNoche > 0 ? hab.PrecioNoche : 0.01m
                });
            }
        }

        return new ReservaCreateDTO
        {
            SucursalGuid = request.SucursalGuid,
            FechaInicio = fechaInicio,
            FechaFin = fechaFin,
            OrigenCanalReserva = request.OrigenCanalReserva ?? string.Empty,
            Observaciones = request.Observaciones,
            EsWalkin = request.EsWalkin,
            Cliente = request.Cliente,
            Habitaciones = lineas
        };
    }
}
