using HotelLux.Reservation.DataAccess.Entities;
using HotelLux.Reservation.DataManagement.Models;

namespace HotelLux.Reservation.DataManagement.Mappers;

public static class ReservaHabitacionDataMapper
{
    public static ReservaHabitacionDataModel ToDataModel(ReservaHabitacionEntity e) => new()
    {
        IdReservaHabitacion = e.IdReservaHabitacion,
        ReservaHabitacionGuid = e.ReservaHabitacionGuid,
        IdReserva = e.IdReserva,
        HabitacionGuid = e.HabitacionGuid,
        TarifaGuid = e.TarifaGuid,
        FechaInicio = e.FechaInicio,
        FechaFin = e.FechaFin,
        NumAdultos = e.NumAdultos,
        NumNinos = e.NumNinos,
        PrecioNocheAplicado = e.PrecioNocheAplicado,
        SubtotalLinea = e.SubtotalLinea,
        ValorIvaLinea = e.ValorIvaLinea,
        DescuentoLinea = e.DescuentoLinea,
        TotalLinea = e.TotalLinea,
        EstadoDetalle = e.EstadoDetalle,
        FechaRegistroUtc = e.FechaRegistroUtc,
        CreadoPorUsuario = e.CreadoPorUsuario,
        ModificadoPorUsuario = e.ModificadoPorUsuario,
        FechaModificacionUtc = e.FechaModificacionUtc,
        ModificacionIp = e.ModificacionIp,
        ServicioOrigen = e.ServicioOrigen
    };

    public static ReservaHabitacionEntity ToEntity(ReservaHabitacionDataModel m) => new()
    {
        IdReservaHabitacion = m.IdReservaHabitacion,
        ReservaHabitacionGuid = m.ReservaHabitacionGuid,
        IdReserva = m.IdReserva,
        HabitacionGuid = m.HabitacionGuid,
        TarifaGuid = m.TarifaGuid,
        FechaInicio = m.FechaInicio,
        FechaFin = m.FechaFin,
        NumAdultos = m.NumAdultos,
        NumNinos = m.NumNinos,
        PrecioNocheAplicado = m.PrecioNocheAplicado,
        SubtotalLinea = m.SubtotalLinea,
        ValorIvaLinea = m.ValorIvaLinea,
        DescuentoLinea = m.DescuentoLinea,
        TotalLinea = m.TotalLinea,
        EstadoDetalle = m.EstadoDetalle,
        FechaRegistroUtc = m.FechaRegistroUtc,
        CreadoPorUsuario = m.CreadoPorUsuario,
        ModificadoPorUsuario = m.ModificadoPorUsuario,
        FechaModificacionUtc = m.FechaModificacionUtc,
        ModificacionIp = m.ModificacionIp,
        ServicioOrigen = m.ServicioOrigen
    };
}
