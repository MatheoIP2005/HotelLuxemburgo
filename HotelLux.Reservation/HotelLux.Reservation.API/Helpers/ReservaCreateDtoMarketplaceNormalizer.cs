using HotelLux.Reservation.Business.DTOs.Reserva;

namespace HotelLux.Reservation.API.Helpers;

/// <summary>
/// Completa importes y líneas cuando el cliente envía el cuerpo con ceros según endpoints_publicas.txt
/// (precio/total opcionales en el contrato público).
/// </summary>
public static class ReservaCreateDtoMarketplaceNormalizer
{
    public static void Apply(ReservaCreateDTO dto)
    {
        if (dto.Habitaciones is null || dto.Habitaciones.Count == 0)
            return;

        foreach (var h in dto.Habitaciones)
        {
            var nights = Math.Max(1, h.FechaFin.DayNumber - h.FechaInicio.DayNumber);

            if (h.PrecioNocheAplicado <= 0)
                h.PrecioNocheAplicado = 0.01m;

            if (h.SubtotalLinea <= 0)
                h.SubtotalLinea = decimal.Round(h.PrecioNocheAplicado * nights, 2, MidpointRounding.AwayFromZero);

            if (h.ValorIvaLinea < 0)
                h.ValorIvaLinea = 0;

            if (h.TotalLinea <= 0)
                h.TotalLinea = decimal.Round(h.SubtotalLinea - h.DescuentoLinea + h.ValorIvaLinea, 2, MidpointRounding.AwayFromZero);

            if (h.TotalLinea <= 0)
                h.TotalLinea = h.SubtotalLinea;
        }

        if (dto.SubtotalReserva <= 0)
            dto.SubtotalReserva = dto.Habitaciones.Sum(h => h.SubtotalLinea);

        if (dto.ValorIva < 0)
            dto.ValorIva = 0;
        if (dto.ValorIva == 0)
            dto.ValorIva = dto.Habitaciones.Sum(h => h.ValorIvaLinea);

        if (dto.TotalReserva <= 0)
            dto.TotalReserva = decimal.Round(
                dto.Habitaciones.Sum(h => h.TotalLinea) - dto.DescuentoAplicado,
                2,
                MidpointRounding.AwayFromZero);

        if (dto.TotalReserva <= 0)
            dto.TotalReserva = decimal.Round(dto.SubtotalReserva + dto.ValorIva - dto.DescuentoAplicado, 2, MidpointRounding.AwayFromZero);

        if (dto.SaldoPendiente <= 0 && dto.TotalReserva > 0)
            dto.SaldoPendiente = dto.TotalReserva;
    }
}
