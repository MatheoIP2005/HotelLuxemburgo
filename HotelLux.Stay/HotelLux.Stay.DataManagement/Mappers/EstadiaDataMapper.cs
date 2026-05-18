using HotelLux.Stay.DataAccess.Entities;
using HotelLux.Stay.DataManagement.Models;

namespace HotelLux.Stay.DataManagement.Mappers;

public static class EstadiaDataMapper
{
    public static EstadiaDataModel ToModel(EstadiaEntity e) => new()
    {
        IdEstadia = e.IdEstadia,
        EstadiaGuid = e.EstadiaGuid,
        ReservaGuid = e.ReservaGuid,
        ReservaHabitacionGuid = e.ReservaHabitacionGuid,
        ClienteGuid = e.ClienteGuid,
        SucursalGuid = e.SucursalGuid,
        HabitacionGuid = e.HabitacionGuid,
        Estado = e.Estado,
        FechaCheckinUtc = e.FechaCheckinUtc,
        FechaCheckoutUtc = e.FechaCheckoutUtc,
        ObservacionesCheckin = e.ObservacionesCheckin,
        ObservacionesCheckout = e.ObservacionesCheckout,
        RequiereMantenimiento = e.RequiereMantenimiento,
        FechaRegistroUtc = e.FechaRegistroUtc,
        CreadoPorUsuario = e.CreadoPorUsuario,
        ModificadoPorUsuario = e.ModificadoPorUsuario,
        FechaModificacionUtc = e.FechaModificacionUtc,
        EsEliminado = e.EsEliminado,
        ServicioOrigen = e.ServicioOrigen
    };

    public static EstadiaEntity ToEntity(EstadiaDataModel m) => new()
    {
        IdEstadia = m.IdEstadia,
        EstadiaGuid = m.EstadiaGuid,
        ReservaGuid = m.ReservaGuid,
        ReservaHabitacionGuid = m.ReservaHabitacionGuid,
        ClienteGuid = m.ClienteGuid,
        SucursalGuid = m.SucursalGuid,
        HabitacionGuid = m.HabitacionGuid,
        Estado = m.Estado,
        FechaCheckinUtc = m.FechaCheckinUtc,
        FechaCheckoutUtc = m.FechaCheckoutUtc,
        ObservacionesCheckin = m.ObservacionesCheckin,
        ObservacionesCheckout = m.ObservacionesCheckout,
        RequiereMantenimiento = m.RequiereMantenimiento,
        FechaRegistroUtc = m.FechaRegistroUtc,
        CreadoPorUsuario = m.CreadoPorUsuario,
        ModificadoPorUsuario = m.ModificadoPorUsuario,
        FechaModificacionUtc = m.FechaModificacionUtc,
        EsEliminado = m.EsEliminado,
        ServicioOrigen = m.ServicioOrigen
    };
}
