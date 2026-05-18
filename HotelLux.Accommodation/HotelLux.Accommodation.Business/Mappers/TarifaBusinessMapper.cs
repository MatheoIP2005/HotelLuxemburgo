using HotelLux.Accommodation.Business.DTOs.Tarifa;
using HotelLux.Accommodation.DataManagement.Models;

namespace HotelLux.Accommodation.Business.Mappers;

public static class TarifaBusinessMapper
{
    public static TarifaDTO ToDTO(TarifaDataModel m) => new()
    {
        TarifaGuid = m.TarifaGuid,
        CodigoTarifa = m.CodigoTarifa,
        SucursalGuid = m.SucursalGuid,
        TipoHabitacionGuid = m.TipoHabitacionGuid,
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
        FechaRegistroUtc = m.FechaRegistroUtc,
        CreadoPorUsuario = m.CreadoPorUsuario
    };

    public static TarifaDataModel ToDataModel(TarifaCreateDTO dto, int idSucursal, int idTipoHabitacion) => new()
    {
        CodigoTarifa = dto.CodigoTarifa,
        IdSucursal = idSucursal,
        IdTipoHabitacion = idTipoHabitacion,
        NombreTarifa = dto.NombreTarifa,
        CanalTarifa = string.IsNullOrWhiteSpace(dto.CanalTarifa) ? "TODOS" : dto.CanalTarifa!,
        FechaInicio = dto.FechaInicio,
        FechaFin = dto.FechaFin,
        PrecioPorNoche = dto.PrecioPorNoche,
        PorcentajeIva = dto.PorcentajeIva ?? 15m,
        MinNoches = dto.MinNoches,
        MaxNoches = dto.MaxNoches,
        PermitePortalPublico = dto.PermitePortalPublico,
        Prioridad = dto.Prioridad ?? 1,
        EstadoTarifa = "ACT",
        FechaRegistroUtc = DateTimeOffset.UtcNow,
        CreadoPorUsuario = dto.CreadoPorUsuario ?? "system",
        ModificacionIp = dto.CreadoDesdeIp,
        ServicioOrigen = "accommodation-service"
    };

    public static TarifaDataModel ToDataModel(TarifaUpdateDTO dto, TarifaDataModel existing)
    {
        existing.NombreTarifa = dto.NombreTarifa;
        existing.CanalTarifa = dto.CanalTarifa;
        existing.FechaInicio = dto.FechaInicio;
        existing.FechaFin = dto.FechaFin;
        existing.PrecioPorNoche = dto.PrecioPorNoche;
        existing.PorcentajeIva = dto.PorcentajeIva;
        existing.MinNoches = dto.MinNoches;
        existing.MaxNoches = dto.MaxNoches;
        existing.PermitePortalPublico = dto.PermitePortalPublico;
        existing.Prioridad = dto.Prioridad;
        existing.EstadoTarifa = dto.EstadoTarifa;
        existing.ModificadoPorUsuario = dto.ModificadoPorUsuario;
        existing.FechaModificacionUtc = DateTimeOffset.UtcNow;
        existing.ModificacionIp = dto.ModificadoDesdeIp;
        return existing;
    }
}
