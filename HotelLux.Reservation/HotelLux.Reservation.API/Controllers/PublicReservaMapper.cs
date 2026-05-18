using HotelLux.Reservation.Business.DTOs.Reserva;

namespace HotelLux.Reservation.API.Controllers;

internal static class PublicReservaMapper
{
    public static object ToPublicReserva(ReservaDTO r) => new
    {
        reservaGuid = r.ReservaGuid,
        codigoReserva = r.CodigoReserva,
        clienteGuid = r.ClienteGuid,
        sucursalGuid = r.SucursalGuid,
        fechaReservaUtc = r.FechaReservaUtc,
        fechaInicio = r.FechaInicio,
        fechaFin = r.FechaFin,
        subtotalReserva = r.SubtotalReserva,
        valorIva = r.ValorIva,
        totalReserva = r.TotalReserva,
        descuentoAplicado = r.DescuentoAplicado,
        saldoPendiente = r.SaldoPendiente,
        origenCanalReserva = r.OrigenCanalReserva,
        estadoReserva = r.EstadoReserva,
        fechaConfirmacionUtc = r.FechaConfirmacionUtc,
        fechaCancelacionUtc = r.FechaCancelacionUtc,
        motivoCancelacion = r.MotivoCancelacion,
        observaciones = r.Observaciones,
        esWalkin = r.EsWalkin,
        habitaciones = r.Habitaciones.Select(h => new
        {
            reservaHabitacionGuid = h.ReservaHabitacionGuid,
            habitacionGuid = h.HabitacionGuid,
            fechaInicio = h.FechaInicio,
            fechaFin = h.FechaFin,
            numAdultos = h.NumAdultos,
            numNinos = h.NumNinos,
            precioNocheAplicado = h.PrecioNocheAplicado,
            subtotalLinea = h.SubtotalLinea,
            valorIvaLinea = h.ValorIvaLinea,
            descuentoLinea = h.DescuentoLinea,
            totalLinea = h.TotalLinea,
            estadoDetalle = h.EstadoDetalle
        }).ToList()
    };
}
