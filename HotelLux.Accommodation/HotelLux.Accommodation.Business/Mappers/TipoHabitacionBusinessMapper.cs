using HotelLux.Accommodation.Business.DTOs.TipoHabitacion;
using HotelLux.Accommodation.DataManagement.Models;

namespace HotelLux.Accommodation.Business.Mappers;

public static class TipoHabitacionBusinessMapper
{
    public static TipoHabitacionDTO ToDTO(TipoHabitacionDataModel m) => new()
    {
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
        FechaRegistroUtc = m.FechaRegistroUtc,
        CreadoPorUsuario = m.CreadoPorUsuario
    };

    public static TipoHabitacionDataModel ToDataModel(TipoHabitacionCreateDTO dto) => new()
    {
        CodigoTipoHabitacion = dto.CodigoTipoHabitacion,
        NombreTipoHabitacion = dto.NombreTipoHabitacion,
        Descripcion = dto.Descripcion,
        CapacidadAdultos = dto.CapacidadAdultos,
        CapacidadNinos = dto.CapacidadNinos,
        CapacidadTotal = dto.CapacidadTotal,
        TipoCama = dto.TipoCama,
        AreaM2 = dto.AreaM2,
        PermiteEventos = dto.PermiteEventos,
        PermiteReservaPublica = dto.PermiteReservaPublica,
        EstadoTipoHabitacion = "ACT",
        FechaRegistroUtc = DateTimeOffset.UtcNow,
        CreadoPorUsuario = dto.CreadoPorUsuario ?? "system",
        ModificacionIp = dto.CreadoDesdeIp,
        ServicioOrigen = "accommodation-service"
    };

    public static TipoHabitacionDataModel ToDataModel(TipoHabitacionUpdateDTO dto, TipoHabitacionDataModel existing)
    {
        existing.CodigoTipoHabitacion = dto.CodigoTipoHabitacion;
        existing.NombreTipoHabitacion = dto.NombreTipoHabitacion;
        existing.Descripcion = dto.Descripcion;
        existing.CapacidadAdultos = dto.CapacidadAdultos;
        existing.CapacidadNinos = dto.CapacidadNinos;
        existing.CapacidadTotal = dto.CapacidadTotal;
        existing.TipoCama = dto.TipoCama;
        existing.AreaM2 = dto.AreaM2;
        existing.PermiteEventos = dto.PermiteEventos;
        existing.PermiteReservaPublica = dto.PermiteReservaPublica;
        existing.EstadoTipoHabitacion = dto.EstadoTipoHabitacion;
        existing.ModificadoPorUsuario = dto.ModificadoPorUsuario;
        existing.FechaModificacionUtc = DateTimeOffset.UtcNow;
        existing.ModificacionIp = dto.ModificadoDesdeIp;
        return existing;
    }
}
