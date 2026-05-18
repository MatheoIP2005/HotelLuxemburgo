using HotelLux.Reservation.Business.DTOs.Reserva;
using HotelLux.Reservation.Business.DTOs.ReservaHabitacion;
using HotelLux.Reservation.DataManagement.Models;

namespace HotelLux.Reservation.Business.Mappers;

public static class ReservaBusinessMapper
{
    public static ReservaDTO ToDTO(ReservaDataModel m) => new()
    {
        ReservaGuid = m.ReservaGuid,
        CodigoReserva = m.CodigoReserva,
        ClienteGuid = m.ClienteGuid,
        SucursalGuid = m.SucursalGuid,
        FechaReservaUtc = m.FechaReservaUtc,
        FechaInicio = m.FechaInicio,
        FechaFin = m.FechaFin,
        SubtotalReserva = m.SubtotalReserva,
        ValorIva = m.ValorIva,
        TotalReserva = m.TotalReserva,
        DescuentoAplicado = m.DescuentoAplicado,
        SaldoPendiente = m.SaldoPendiente,
        OrigenCanalReserva = m.OrigenCanalReserva,
        EstadoReserva = m.EstadoReserva,
        FechaConfirmacionUtc = m.FechaConfirmacionUtc,
        FechaCancelacionUtc = m.FechaCancelacionUtc,
        MotivoCancelacion = m.MotivoCancelacion,
        Observaciones = m.Observaciones,
        EsWalkin = m.EsWalkin,
        FechaRegistroUtc = m.FechaRegistroUtc,
        CreadoPorUsuario = m.CreadoPorUsuario,
        Habitaciones = m.Habitaciones.Select(ReservaHabitacionBusinessMapper.ToDTO).ToList()
    };

    public static ReservaDataModel ToDataModel(ReservaCreateDTO dto) => new()
    {
        ClienteGuid = dto.ClienteGuid!.Value,
        SucursalGuid = dto.SucursalGuid,
        FechaReservaUtc = DateTimeOffset.UtcNow,
        FechaInicio = dto.FechaInicio,
        FechaFin = dto.FechaFin,
        SubtotalReserva = dto.SubtotalReserva,
        ValorIva = dto.ValorIva,
        TotalReserva = dto.TotalReserva,
        DescuentoAplicado = dto.DescuentoAplicado,
        SaldoPendiente = dto.SaldoPendiente,
        OrigenCanalReserva = dto.OrigenCanalReserva,
        Observaciones = dto.Observaciones,
        EsWalkin = dto.EsWalkin,
        CreadoPorUsuario = dto.CreadoPorUsuario ?? "api_user",
        CreadoDesdeIp = dto.CreadoDesdeIp,
        EstadoReserva = "PEN",
        ServicioOrigen = "reservation-service",
        Habitaciones = dto.Habitaciones.Select(h => new ReservaHabitacionDataModel
        {
            HabitacionGuid = h.HabitacionGuid,
            TarifaGuid = h.TarifaGuid,
            FechaInicio = h.FechaInicio,
            FechaFin = h.FechaFin,
            NumAdultos = h.NumAdultos,
            NumNinos = h.NumNinos,
            PrecioNocheAplicado = h.PrecioNocheAplicado,
            SubtotalLinea = h.SubtotalLinea,
            ValorIvaLinea = h.ValorIvaLinea,
            DescuentoLinea = h.DescuentoLinea,
            TotalLinea = h.TotalLinea,
            CreadoPorUsuario = dto.CreadoPorUsuario ?? "api_user",
            ServicioOrigen = "reservation-service"
        }).ToList()
    };
}
