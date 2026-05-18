using HotelLux.Reservation.Business.DTOs.ReservaHabitacion;
using HotelLux.Reservation.DataManagement.Models;

namespace HotelLux.Reservation.Business.Mappers;

public static class ReservaHabitacionBusinessMapper
{
    public static ReservaHabitacionDTO ToDTO(ReservaHabitacionDataModel m) => new()
    {
        ReservaHabitacionGuid = m.ReservaHabitacionGuid,
        HabitacionGuid = m.HabitacionGuid,
        TarifaGuid = m.TarifaGuid,
        FechaInicio = m.FechaInicio,
        FechaFin = m.FechaFin,
        NumAdultos = m.NumAdultos,
        NumNinos = m.NumNinos,
        PrecioNocheAplicado = m.PrecioNocheAplicado,
        SubtotalLinea = m.SubtotalLinea,
        ValorIvaLinea = m.ValorIvaLinea,
        DescuentoLinea = m.DescuentoLinea,
        TotalLinea = m.TotalLinea,
        EstadoDetalle = m.EstadoDetalle
    };

    public static ReservaHabitacionDataModel FromCreateDto(ReservaHabitacionCreateDTO dto, string usuario) => new()
    {
        HabitacionGuid = dto.HabitacionGuid,
        TarifaGuid = dto.TarifaGuid,
        FechaInicio = dto.FechaInicio,
        FechaFin = dto.FechaFin,
        NumAdultos = dto.NumAdultos,
        NumNinos = dto.NumNinos,
        PrecioNocheAplicado = dto.PrecioNocheAplicado,
        SubtotalLinea = dto.SubtotalLinea,
        ValorIvaLinea = dto.ValorIvaLinea,
        DescuentoLinea = dto.DescuentoLinea,
        TotalLinea = dto.TotalLinea,
        CreadoPorUsuario = usuario,
        ServicioOrigen = "reservation-service"
    };
}
