using HotelLux.Accommodation.DataAccess.Entities;
using HotelLux.Accommodation.DataManagement.Models;

namespace HotelLux.Accommodation.DataManagement.Mappers;

public static class HabitacionDataMapper
{
    public static HabitacionDataModel ToDataModel(HabitacionEntity e) => new()
    {
        IdHabitacion = e.IdHabitacion,
        HabitacionGuid = e.HabitacionGuid,
        IdSucursal = e.IdSucursal,
        IdTipoHabitacion = e.IdTipoHabitacion,
        SucursalGuid = e.Sucursal?.SucursalGuid ?? Guid.Empty,
        TipoHabitacionGuid = e.TipoHabitacion?.TipoHabitacionGuid ?? Guid.Empty,
        NumeroHabitacion = e.NumeroHabitacion,
        Piso = e.Piso,
        CapacidadHabitacion = e.CapacidadHabitacion,
        PrecioBase = e.PrecioBase,
        DescripcionHabitacion = e.DescripcionHabitacion,
        EstadoHabitacion = e.EstadoHabitacion,
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

    public static HabitacionEntity ToEntity(HabitacionDataModel m) => new()
    {
        IdHabitacion = m.IdHabitacion,
        HabitacionGuid = m.HabitacionGuid,
        IdSucursal = m.IdSucursal,
        IdTipoHabitacion = m.IdTipoHabitacion,
        NumeroHabitacion = m.NumeroHabitacion,
        Piso = m.Piso,
        CapacidadHabitacion = m.CapacidadHabitacion,
        PrecioBase = m.PrecioBase,
        DescripcionHabitacion = m.DescripcionHabitacion,
        EstadoHabitacion = m.EstadoHabitacion,
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
