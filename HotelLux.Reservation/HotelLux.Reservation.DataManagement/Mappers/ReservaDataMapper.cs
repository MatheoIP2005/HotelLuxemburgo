using HotelLux.Reservation.DataAccess.Entities;
using HotelLux.Reservation.DataManagement.Models;

namespace HotelLux.Reservation.DataManagement.Mappers;

public static class ReservaDataMapper
{
    public static ReservaDataModel ToDataModel(ReservaEntity e) => new()
    {
        IdReserva = e.IdReserva,
        ReservaGuid = e.ReservaGuid,
        CodigoReserva = e.CodigoReserva,
        IdCliente = e.IdCliente,
        ClienteGuid = e.Cliente?.ClienteGuid ?? e.ClienteGuid,
        SucursalGuid = e.SucursalGuid,
        FechaReservaUtc = e.FechaReservaUtc,
        FechaInicio = e.FechaInicio,
        FechaFin = e.FechaFin,
        SubtotalReserva = e.SubtotalReserva,
        ValorIva = e.ValorIva,
        TotalReserva = e.TotalReserva,
        DescuentoAplicado = e.DescuentoAplicado,
        SaldoPendiente = e.SaldoPendiente,
        OrigenCanalReserva = e.OrigenCanalReserva,
        EstadoReserva = e.EstadoReserva,
        FechaConfirmacionUtc = e.FechaConfirmacionUtc,
        FechaCancelacionUtc = e.FechaCancelacionUtc,
        MotivoCancelacion = e.MotivoCancelacion,
        Observaciones = e.Observaciones,
        EsWalkin = e.EsWalkin,
        EsEliminado = e.EsEliminado,
        FechaInhabilitacionUtc = e.FechaInhabilitacionUtc,
        MotivoInhabilitacion = e.MotivoInhabilitacion,
        FechaRegistroUtc = e.FechaRegistroUtc,
        CreadoPorUsuario = e.CreadoPorUsuario,
        CreadoDesdeIp = e.CreadoDesdeIp,
        ModificadoPorUsuario = e.ModificadoPorUsuario,
        FechaModificacionUtc = e.FechaModificacionUtc,
        ModificacionIp = e.ModificacionIp,
        ServicioOrigen = e.ServicioOrigen,
        Habitaciones = e.ReservasHabitaciones
            .Select(ReservaHabitacionDataMapper.ToDataModel)
            .ToList()
    };

    public static ReservaEntity ToEntity(ReservaDataModel m) => new()
    {
        IdReserva = m.IdReserva,
        ReservaGuid = m.ReservaGuid,
        CodigoReserva = m.CodigoReserva,
        IdCliente = m.IdCliente,
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
        EsEliminado = m.EsEliminado,
        FechaInhabilitacionUtc = m.FechaInhabilitacionUtc,
        MotivoInhabilitacion = m.MotivoInhabilitacion,
        FechaRegistroUtc = m.FechaRegistroUtc,
        CreadoPorUsuario = m.CreadoPorUsuario,
        CreadoDesdeIp = m.CreadoDesdeIp,
        ModificadoPorUsuario = m.ModificadoPorUsuario,
        FechaModificacionUtc = m.FechaModificacionUtc,
        ModificacionIp = m.ModificacionIp,
        ServicioOrigen = m.ServicioOrigen
    };
}
