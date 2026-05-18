using HotelLux.Accommodation.Business.DTOs.Habitacion;

namespace HotelLux.Accommodation.Business.Validators;

public static class HabitacionValidator
{
    public static List<string> ValidarCreacion(HabitacionCreateDTO dto)
    {
        var e = new List<string>();
        if (dto.SucursalGuid == Guid.Empty) e.Add("SucursalGuid es requerido.");
        if (dto.TipoHabitacionGuid == Guid.Empty) e.Add("TipoHabitacionGuid es requerido.");
        if (string.IsNullOrWhiteSpace(dto.NumeroHabitacion)) e.Add("NumeroHabitacion es requerido.");
        if (dto.CapacidadHabitacion <= 0) e.Add("CapacidadHabitacion debe ser mayor a 0.");
        if (dto.PrecioBase < 0) e.Add("PrecioBase no puede ser negativo.");
        return e;
    }

    public static List<string> ValidarActualizacion(HabitacionUpdateDTO dto)
    {
        var e = new List<string>();
        if (string.IsNullOrWhiteSpace(dto.NumeroHabitacion)) e.Add("NumeroHabitacion es requerido.");
        if (dto.CapacidadHabitacion <= 0) e.Add("CapacidadHabitacion debe ser mayor a 0.");
        if (dto.PrecioBase < 0) e.Add("PrecioBase no puede ser negativo.");
        return e;
    }
}
