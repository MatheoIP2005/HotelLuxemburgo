using HotelLux.Accommodation.DataAccess.Entities;
using HotelLux.Accommodation.DataManagement.Models;

namespace HotelLux.Accommodation.DataManagement.Mappers;

public static class TipoHabitacionImagenDataMapper
{
    public static TipoHabitacionImagenDataModel ToDataModel(TipoHabitacionImagenEntity e) => new()
    {
        IdTipoHabitacionImagen = e.IdTipoHabitacionImagen,
        IdTipoHabitacion = e.IdTipoHabitacion,
        UrlImagen = e.UrlImagen,
        DescripcionImagen = e.DescripcionImagen,
        OrdenVisualizacion = e.OrdenVisualizacion,
        EsPrincipal = e.EsPrincipal,
        FechaRegistroUtc = e.FechaRegistroUtc,
        CreadoPorUsuario = e.CreadoPorUsuario
    };

    public static TipoHabitacionImagenEntity ToEntity(TipoHabitacionImagenDataModel m) => new()
    {
        IdTipoHabitacionImagen = m.IdTipoHabitacionImagen,
        IdTipoHabitacion = m.IdTipoHabitacion,
        UrlImagen = m.UrlImagen,
        DescripcionImagen = m.DescripcionImagen,
        OrdenVisualizacion = m.OrdenVisualizacion,
        EsPrincipal = m.EsPrincipal,
        FechaRegistroUtc = m.FechaRegistroUtc,
        CreadoPorUsuario = m.CreadoPorUsuario
    };
}
