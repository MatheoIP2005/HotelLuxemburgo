using HotelLux.Reservation.Business.DTOs.ReservaHabitacion;
using HotelLux.Reservation.Business.Validators;
using Xunit;

namespace HotelLux.Reservation.Business.Tests;

public class ReservaValidatorLineaTests
{
    [Fact]
    public void Linea_valida_sin_errores()
    {
        var dto = new ReservaHabitacionCreateDTO
        {
            HabitacionGuid = Guid.NewGuid(),
            FechaInicio = new DateOnly(2026, 6, 1),
            FechaFin = new DateOnly(2026, 6, 5),
            NumAdultos = 2,
            PrecioNocheAplicado = 50,
            SubtotalLinea = 200,
            ValorIvaLinea = 0,
            DescuentoLinea = 0,
            TotalLinea = 200
        };
        Assert.Empty(ReservaValidator.ValidarLineaHabitacion(dto));
    }

    [Fact]
    public void Linea_sin_total_falla()
    {
        var dto = new ReservaHabitacionCreateDTO
        {
            HabitacionGuid = Guid.NewGuid(),
            FechaInicio = new DateOnly(2026, 6, 1),
            FechaFin = new DateOnly(2026, 6, 5),
            NumAdultos = 1,
            PrecioNocheAplicado = 10,
            SubtotalLinea = 0,
            ValorIvaLinea = 0,
            DescuentoLinea = 0,
            TotalLinea = 0
        };
        Assert.NotEmpty(ReservaValidator.ValidarLineaHabitacion(dto));
    }
}
