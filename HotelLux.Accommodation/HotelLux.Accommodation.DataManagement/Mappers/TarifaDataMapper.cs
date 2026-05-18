using HotelLux.Accommodation.DataAccess.Entities;
using HotelLux.Accommodation.DataManagement.Models;

namespace HotelLux.Accommodation.DataManagement.Mappers;

public static class TarifaDataMapper
{
    public static TarifaDataModel ToDataModel(TarifaEntity e) => new()
    {
        IdTarifa = e.IdTarifa,
        TarifaGuid = e.TarifaGuid,
        CodigoTarifa = e.CodigoTarifa,
        IdSucursal = e.IdSucursal,
        IdTipoHabitacion = e.IdTipoHabitacion,
        SucursalGuid = e.Sucursal?.SucursalGuid ?? Guid.Empty,
        TipoHabitacionGuid = e.TipoHabitacion?.TipoHabitacionGuid ?? Guid.Empty,
        NombreTarifa = e.NombreTarifa,
        CanalTarifa = e.CanalTarifa,
        FechaInicio = e.FechaInicio,
        FechaFin = e.FechaFin,
        PrecioPorNoche = e.PrecioPorNoche,
        PorcentajeIva = e.PorcentajeIva,
        MinNoches = e.MinNoches,
        MaxNoches = e.MaxNoches,
        PermitePortalPublico = e.PermitePortalPublico,
        Prioridad = e.Prioridad,
        EstadoTarifa = e.EstadoTarifa,
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

    public static TarifaEntity ToEntity(TarifaDataModel m) => new()
    {
        IdTarifa = m.IdTarifa,
        TarifaGuid = m.TarifaGuid,
        CodigoTarifa = m.CodigoTarifa,
        IdSucursal = m.IdSucursal,
        IdTipoHabitacion = m.IdTipoHabitacion,
        NombreTarifa = m.NombreTarifa,
        CanalTarifa = m.CanalTarifa,
        FechaInicio = m.FechaInicio,
        FechaFin = m.FechaFin,
        PrecioPorNoche = m.PrecioPorNoche,
        PorcentajeIva = m.PorcentajeIva,
        MinNoches = m.MinNoches,
        MaxNoches = m.MaxNoches,
        PermitePortalPublico = m.PermitePortalPublico,
        Prioridad = m.Prioridad,
        EstadoTarifa = m.EstadoTarifa,
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
