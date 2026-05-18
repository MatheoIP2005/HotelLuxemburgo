using HotelLux.Accommodation.Business.DTOs.Tarifa;

namespace HotelLux.Accommodation.Business.Validators;

public static class TarifaValidator
{
    public static List<string> ValidarCreacion(TarifaCreateDTO dto)
    {
        var e = new List<string>();
        if (string.IsNullOrWhiteSpace(dto.CodigoTarifa)) e.Add("CodigoTarifa es requerido.");
        if (string.IsNullOrWhiteSpace(dto.NombreTarifa)) e.Add("NombreTarifa es requerido.");
        if (dto.SucursalGuid == Guid.Empty) e.Add("SucursalGuid es requerido.");
        if (dto.TipoHabitacionGuid == Guid.Empty) e.Add("TipoHabitacionGuid es requerido.");
        if (dto.FechaFin <= dto.FechaInicio) e.Add("FechaFin debe ser posterior a FechaInicio.");
        if (dto.PrecioPorNoche <= 0) e.Add("PrecioPorNoche debe ser mayor a 0.");
        if (dto.MinNoches < 1) e.Add("MinNoches debe ser al menos 1.");
        return e;
    }

    public static List<string> ValidarActualizacion(TarifaUpdateDTO dto)
    {
        var e = new List<string>();
        if (string.IsNullOrWhiteSpace(dto.NombreTarifa)) e.Add("NombreTarifa es requerido.");
        if (dto.FechaFin <= dto.FechaInicio) e.Add("FechaFin debe ser posterior a FechaInicio.");
        if (dto.PrecioPorNoche <= 0) e.Add("PrecioPorNoche debe ser mayor a 0.");
        return e;
    }
}
