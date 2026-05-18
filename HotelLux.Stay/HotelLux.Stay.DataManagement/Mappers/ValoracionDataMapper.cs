using HotelLux.Stay.DataAccess.Entities;
using HotelLux.Stay.DataManagement.Models;

namespace HotelLux.Stay.DataManagement.Mappers;

public static class ValoracionDataMapper
{
    public static ValoracionDataModel ToModel(ValoracionEntity e) => new()
    {
        IdValoracion           = e.IdValoracion,
        ValoracionGuid         = e.ValoracionGuid,
        EstadiaGuid            = e.EstadiaGuid,
        SucursalGuid           = e.SucursalGuid,
        ClienteGuid            = e.ClienteGuid,
        PuntuacionGeneral      = e.PuntuacionGeneral,
        PuntuacionLimpieza     = e.PuntuacionLimpieza,
        PuntuacionConfort      = e.PuntuacionConfort,
        PuntuacionUbicacion    = e.PuntuacionUbicacion,
        PuntuacionInstalaciones = e.PuntuacionInstalaciones,
        PuntuacionPersonal     = e.PuntuacionPersonal,
        PuntuacionCalidadPrecio = e.PuntuacionCalidadPrecio,
        ComentarioPositivo     = e.ComentarioPositivo,
        ComentarioNegativo     = e.ComentarioNegativo,
        TipoViaje              = e.TipoViaje,
        FechaPublicacionUtc    = e.FechaPublicacionUtc,
        RespuestaHotel         = e.RespuestaHotel,
        NombreVisibleCliente   = e.NombreVisibleCliente,
        EsEliminado            = e.EsEliminado,
        FechaRegistroUtc       = e.FechaRegistroUtc,
        CreadoPorUsuario       = e.CreadoPorUsuario
    };

    public static ValoracionEntity ToEntity(ValoracionDataModel m) => new()
    {
        ValoracionGuid = m.ValoracionGuid == Guid.Empty ? Guid.NewGuid() : m.ValoracionGuid,
        EstadiaGuid    = m.EstadiaGuid,
        SucursalGuid   = m.SucursalGuid,
        ClienteGuid    = m.ClienteGuid,
        PuntuacionGeneral      = m.PuntuacionGeneral,
        PuntuacionLimpieza     = m.PuntuacionLimpieza,
        PuntuacionConfort      = m.PuntuacionConfort,
        PuntuacionUbicacion    = m.PuntuacionUbicacion,
        PuntuacionInstalaciones = m.PuntuacionInstalaciones,
        PuntuacionPersonal     = m.PuntuacionPersonal,
        PuntuacionCalidadPrecio = m.PuntuacionCalidadPrecio,
        ComentarioPositivo = m.ComentarioPositivo,
        ComentarioNegativo = m.ComentarioNegativo,
        TipoViaje          = m.TipoViaje,
        FechaPublicacionUtc = m.FechaPublicacionUtc,
        RespuestaHotel     = m.RespuestaHotel,
        NombreVisibleCliente = m.NombreVisibleCliente,
        EsEliminado        = false,
        FechaRegistroUtc   = DateTimeOffset.UtcNow,
        CreadoPorUsuario   = m.CreadoPorUsuario ?? "stay_api"
    };
}
