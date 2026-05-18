using HotelLux.Accommodation.Business.DTOs.TipoHabitacionImagen;
using HotelLux.Accommodation.DataManagement.Models;

namespace HotelLux.Accommodation.Business.Mappers;

public static class TipoHabitacionImagenBusinessMapper
{
    public static TipoHabitacionImagenDTO ToDTO(TipoHabitacionImagenDataModel m) => new()
    {
        IdTipoHabitacionImagen = m.IdTipoHabitacionImagen,
        UrlImagen = m.UrlImagen,
        DescripcionImagen = m.DescripcionImagen,
        OrdenVisualizacion = m.OrdenVisualizacion,
        EsPrincipal = m.EsPrincipal,
        FechaRegistroUtc = m.FechaRegistroUtc
    };

    public static TipoHabitacionImagenDataModel ToDataModel(TipoHabitacionImagenCreateDTO dto, int idTipoHabitacion) => new()
    {
        IdTipoHabitacion = idTipoHabitacion,
        UrlImagen = dto.UrlImagen,
        DescripcionImagen = dto.DescripcionImagen,
        OrdenVisualizacion = dto.OrdenVisualizacion,
        EsPrincipal = dto.EsPrincipal,
        FechaRegistroUtc = DateTimeOffset.UtcNow,
        CreadoPorUsuario = dto.CreadoPorUsuario ?? "system"
    };
}
