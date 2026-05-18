using HotelLux.Accommodation.DataAccess.Entities;
using HotelLux.Accommodation.DataManagement.Models;

namespace HotelLux.Accommodation.DataManagement.Mappers;

public static class CatalogoServicioDataMapper
{
    public static CatalogoServicioDataModel ToDataModel(CatalogoServicioEntity e) => new()
    {
        IdCatalogo = e.IdCatalogo,
        CatalogoGuid = e.CatalogoGuid,
        IdSucursal = e.IdSucursal,
        SucursalGuid = e.Sucursal?.SucursalGuid,
        CodigoCatalogo = e.CodigoCatalogo,
        NombreCatalogo = e.NombreCatalogo,
        TipoCatalogo = e.TipoCatalogo,
        CategoriaCatalogo = e.CategoriaCatalogo,
        DescripcionCatalogo = e.DescripcionCatalogo,
        PrecioBase = e.PrecioBase,
        AplicaIva = e.AplicaIva,
        Disponible24h = e.Disponible24h,
        HoraInicio = e.HoraInicio,
        HoraFin = e.HoraFin,
        IconoUrl = e.IconoUrl,
        EstadoCatalogo = e.EstadoCatalogo,
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

    public static CatalogoServicioEntity ToEntity(CatalogoServicioDataModel m) => new()
    {
        IdCatalogo = m.IdCatalogo,
        CatalogoGuid = m.CatalogoGuid,
        IdSucursal = m.IdSucursal,
        CodigoCatalogo = m.CodigoCatalogo,
        NombreCatalogo = m.NombreCatalogo,
        TipoCatalogo = m.TipoCatalogo,
        CategoriaCatalogo = m.CategoriaCatalogo,
        DescripcionCatalogo = m.DescripcionCatalogo,
        PrecioBase = m.PrecioBase,
        AplicaIva = m.AplicaIva,
        Disponible24h = m.Disponible24h,
        HoraInicio = m.HoraInicio,
        HoraFin = m.HoraFin,
        IconoUrl = m.IconoUrl,
        EstadoCatalogo = m.EstadoCatalogo,
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
