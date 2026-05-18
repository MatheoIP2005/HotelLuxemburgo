using HotelLux.Accommodation.DataAccess.Entities;
using HotelLux.Accommodation.DataManagement.Models;

namespace HotelLux.Accommodation.DataManagement.Mappers;

public static class SucursalImagenDataMapper
{
    public static SucursalImagenDataModel ToDataModel(SucursalImagenEntity e) => new()
    {
        IdSucursalImagen = e.IdSucursalImagen,
        SucursalImagenGuid = e.SucursalImagenGuid,
        IdSucursal = e.IdSucursal,
        UrlImagen = e.UrlImagen,
        DescripcionImagen = e.DescripcionImagen,
        OrdenVisualizacion = e.OrdenVisualizacion,
        EsPrincipal = e.EsPrincipal,
        FechaRegistroUtc = e.FechaRegistroUtc,
        CreadoPorUsuario = e.CreadoPorUsuario
    };

    public static SucursalImagenEntity ToEntity(SucursalImagenDataModel m) => new()
    {
        IdSucursalImagen = m.IdSucursalImagen,
        SucursalImagenGuid = m.SucursalImagenGuid,
        IdSucursal = m.IdSucursal,
        UrlImagen = m.UrlImagen,
        DescripcionImagen = m.DescripcionImagen,
        OrdenVisualizacion = m.OrdenVisualizacion,
        EsPrincipal = m.EsPrincipal,
        FechaRegistroUtc = m.FechaRegistroUtc,
        CreadoPorUsuario = m.CreadoPorUsuario
    };
}
