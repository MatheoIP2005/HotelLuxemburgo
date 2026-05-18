using HotelLux.Accommodation.Business.DTOs.Habitacion;
using HotelLux.Accommodation.DataManagement.Models;

namespace HotelLux.Accommodation.Business.Mappers;

public static class HabitacionBusinessMapper
{
    public static HabitacionDTO ToDTO(HabitacionDataModel m) => new()
    {
        HabitacionGuid = m.HabitacionGuid,
        SucursalGuid = m.SucursalGuid,
        TipoHabitacionGuid = m.TipoHabitacionGuid,
        NumeroHabitacion = m.NumeroHabitacion,
        Piso = m.Piso,
        CapacidadHabitacion = m.CapacidadHabitacion,
        PrecioBase = m.PrecioBase,
        DescripcionHabitacion = m.DescripcionHabitacion,
        EstadoHabitacion = m.EstadoHabitacion,
        FechaRegistroUtc = m.FechaRegistroUtc,
        CreadoPorUsuario = m.CreadoPorUsuario
    };

    public static HabitacionDataModel ToDataModel(HabitacionCreateDTO dto, int idSucursal, int idTipoHabitacion) => new()
    {
        IdSucursal = idSucursal,
        IdTipoHabitacion = idTipoHabitacion,
        NumeroHabitacion = dto.NumeroHabitacion,
        Piso = dto.Piso,
        CapacidadHabitacion = dto.CapacidadHabitacion,
        PrecioBase = dto.PrecioBase,
        DescripcionHabitacion = dto.DescripcionHabitacion,
        EstadoHabitacion = "DIS",
        FechaRegistroUtc = DateTimeOffset.UtcNow,
        CreadoPorUsuario = dto.CreadoPorUsuario ?? "system",
        ModificacionIp = dto.CreadoDesdeIp,
        ServicioOrigen = "accommodation-service"
    };

    public static HabitacionDataModel ToDataModel(HabitacionUpdateDTO dto, HabitacionDataModel existing)
    {
        existing.NumeroHabitacion = dto.NumeroHabitacion;
        existing.Piso = dto.Piso;
        existing.CapacidadHabitacion = dto.CapacidadHabitacion;
        existing.PrecioBase = dto.PrecioBase;
        existing.DescripcionHabitacion = dto.DescripcionHabitacion;
        existing.EstadoHabitacion = dto.EstadoHabitacion;
        existing.ModificadoPorUsuario = dto.ModificadoPorUsuario;
        existing.FechaModificacionUtc = DateTimeOffset.UtcNow;
        existing.ModificacionIp = dto.ModificadoDesdeIp;
        return existing;
    }
}
