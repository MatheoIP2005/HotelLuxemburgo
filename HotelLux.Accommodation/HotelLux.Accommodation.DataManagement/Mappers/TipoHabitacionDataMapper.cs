using HotelLux.Accommodation.DataAccess.Entities;
using HotelLux.Accommodation.DataManagement.Models;

namespace HotelLux.Accommodation.DataManagement.Mappers;

public static class TipoHabitacionDataMapper
{
    public static TipoHabitacionDataModel ToDataModel(TipoHabitacionEntity e) => new()
    {
        IdTipoHabitacion = e.IdTipoHabitacion,
        TipoHabitacionGuid = e.TipoHabitacionGuid,
        CodigoTipoHabitacion = e.CodigoTipoHabitacion,
        NombreTipoHabitacion = e.NombreTipoHabitacion,
        Descripcion = e.Descripcion,
        CapacidadAdultos = e.CapacidadAdultos,
        CapacidadNinos = e.CapacidadNinos,
        CapacidadTotal = e.CapacidadTotal,
        TipoCama = e.TipoCama,
        AreaM2 = e.AreaM2,
        PermiteEventos = e.PermiteEventos,
        PermiteReservaPublica = e.PermiteReservaPublica,
        EstadoTipoHabitacion = e.EstadoTipoHabitacion,
        EsEliminado = e.EsEliminado,
        FechaInhabilitacionUtc = e.FechaInhabilitacionUtc,
        MotivoInhabilitacion = e.MotivoInhabilitacion,
        FechaRegistroUtc = e.FechaRegistroUtc,
        CreadoPorUsuario = e.CreadoPorUsuario,
        ModificadoPorUsuario = e.ModificadoPorUsuario,
        FechaModificacionUtc = e.FechaModificacionUtc,
        ModificacionIp = e.ModificacionIp,
        ServicioOrigen = e.ServicioOrigen
    };

    public static TipoHabitacionEntity ToEntity(TipoHabitacionDataModel m) => new()
    {
        IdTipoHabitacion = m.IdTipoHabitacion,
        TipoHabitacionGuid = m.TipoHabitacionGuid,
        CodigoTipoHabitacion = m.CodigoTipoHabitacion,
        NombreTipoHabitacion = m.NombreTipoHabitacion,
        Descripcion = m.Descripcion,
        CapacidadAdultos = m.CapacidadAdultos,
        CapacidadNinos = m.CapacidadNinos,
        CapacidadTotal = m.CapacidadTotal,
        TipoCama = m.TipoCama,
        AreaM2 = m.AreaM2,
        PermiteEventos = m.PermiteEventos,
        PermiteReservaPublica = m.PermiteReservaPublica,
        EstadoTipoHabitacion = m.EstadoTipoHabitacion,
        EsEliminado = m.EsEliminado,
        FechaInhabilitacionUtc = m.FechaInhabilitacionUtc,
        MotivoInhabilitacion = m.MotivoInhabilitacion,
        FechaRegistroUtc = m.FechaRegistroUtc,
        CreadoPorUsuario = m.CreadoPorUsuario,
        ModificadoPorUsuario = m.ModificadoPorUsuario,
        FechaModificacionUtc = m.FechaModificacionUtc,
        ModificacionIp = m.ModificacionIp,
        ServicioOrigen = m.ServicioOrigen
    };
}
