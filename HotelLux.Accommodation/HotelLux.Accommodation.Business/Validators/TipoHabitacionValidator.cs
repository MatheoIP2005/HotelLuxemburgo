using HotelLux.Accommodation.Business.DTOs.TipoHabitacion;

namespace HotelLux.Accommodation.Business.Validators;

public static class TipoHabitacionValidator
{
    public static List<string> ValidarCreacion(TipoHabitacionCreateDTO dto)
    {
        var e = new List<string>();
        if (string.IsNullOrWhiteSpace(dto.CodigoTipoHabitacion)) e.Add("CodigoTipoHabitacion es requerido.");
        if (string.IsNullOrWhiteSpace(dto.NombreTipoHabitacion)) e.Add("NombreTipoHabitacion es requerido.");
        if (dto.CapacidadAdultos <= 0) e.Add("CapacidadAdultos debe ser mayor a 0.");
        if (dto.CapacidadTotal <= 0) e.Add("CapacidadTotal debe ser mayor a 0.");
        return e;
    }

    public static List<string> ValidarActualizacion(TipoHabitacionUpdateDTO dto) => ValidarCreacion(
        new TipoHabitacionCreateDTO
        {
            CodigoTipoHabitacion = dto.CodigoTipoHabitacion,
            NombreTipoHabitacion = dto.NombreTipoHabitacion,
            CapacidadAdultos = dto.CapacidadAdultos,
            CapacidadTotal = dto.CapacidadTotal
        });
}
