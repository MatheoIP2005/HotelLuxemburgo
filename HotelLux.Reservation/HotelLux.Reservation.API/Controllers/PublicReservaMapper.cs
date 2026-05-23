using HotelLux.Reservation.Business.DTOs.Reserva;

namespace HotelLux.Reservation.API.Controllers;

internal static class PublicReservaMapper
{
    /// <summary>Respuesta GET/POST público según endpoints_publicas.txt.</summary>
    public static object ToPublicReserva(ReservaDTO r) => new
    {
        reservaGuid = r.ReservaGuid,
        codigoReserva = r.CodigoReserva,
        clienteGuid = r.ClienteGuid,
        sucursalGuid = r.SucursalGuid,
        fechaReservaUtc = r.FechaReservaUtc,
        fechaInicio = ToUtcDateTimeOffset(r.FechaInicio),
        fechaFin = ToUtcDateTimeOffset(r.FechaFin),
        subtotalReserva = r.SubtotalReserva,
        valorIva = r.ValorIva,
        totalReserva = r.TotalReserva,
        saldoPendiente = r.SaldoPendiente,
        origenCanalReserva = r.OrigenCanalReserva,
        estadoReserva = r.EstadoReserva,
        fechaConfirmacionUtc = r.FechaConfirmacionUtc,
        fechaCancelacionUtc = r.FechaCancelacionUtc,
        motivoCancelacion = r.MotivoCancelacion,
        observaciones = r.Observaciones,
        habitaciones = r.Habitaciones.Select(h => new
        {
            reservaHabitacionGuid = h.ReservaHabitacionGuid,
            habitacionGuid = h.HabitacionGuid,
            fechaInicio = ToUtcDateTimeOffset(h.FechaInicio),
            fechaFin = ToUtcDateTimeOffset(h.FechaFin),
            numAdultos = h.NumAdultos,
            numNinos = h.NumNinos,
            precioNocheAplicado = h.PrecioNocheAplicado,
            subtotalLinea = h.SubtotalLinea,
            valorIvaLinea = h.ValorIvaLinea,
            totalLinea = h.TotalLinea,
            estadoDetalle = h.EstadoDetalle
        }).ToList()
    };

    private static DateTimeOffset ToUtcDateTimeOffset(DateOnly date) =>
        new(date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
}
