using HotelLux.Accommodation.DataAccess.Entities;
using HotelLux.Accommodation.DataManagement.Models;

namespace HotelLux.Accommodation.DataManagement.Mappers;

public static class TipoHabitacionCatalogoDataMapper
{
    public static TipoHabitacionCatalogoDataModel ToDataModel(TipoHabitacionCatalogoEntity e) => new()
    {
        IdTipoHabCatalogo = e.IdTipoHabCatalogo,
        IdTipoHabitacion = e.IdTipoHabitacion,
        IdCatalogo = e.IdCatalogo,
        FechaRegistroUtc = e.FechaRegistroUtc,
        CreadoPorUsuario = e.CreadoPorUsuario
    };

    public static TipoHabitacionCatalogoEntity ToEntity(TipoHabitacionCatalogoDataModel m) => new()
    {
        IdTipoHabCatalogo = m.IdTipoHabCatalogo,
        IdTipoHabitacion = m.IdTipoHabitacion,
        IdCatalogo = m.IdCatalogo,
        FechaRegistroUtc = m.FechaRegistroUtc,
        CreadoPorUsuario = m.CreadoPorUsuario
    };
}
