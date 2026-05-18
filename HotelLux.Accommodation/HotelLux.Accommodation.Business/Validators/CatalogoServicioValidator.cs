using HotelLux.Accommodation.Business.DTOs.CatalogoServicio;

namespace HotelLux.Accommodation.Business.Validators;

public static class CatalogoServicioValidator
{
    private static readonly string[] TiposValidos = ["AME", "SRV"];

    public static List<string> ValidarCreacion(CatalogoServicioCreateDTO dto)
    {
        var e = new List<string>();
        if (string.IsNullOrWhiteSpace(dto.CodigoCatalogo)) e.Add("CodigoCatalogo es requerido.");
        if (string.IsNullOrWhiteSpace(dto.NombreCatalogo)) e.Add("NombreCatalogo es requerido.");
        if (!TiposValidos.Contains(dto.TipoCatalogo)) e.Add("TipoCatalogo debe ser AME o SRV.");
        if (string.IsNullOrWhiteSpace(dto.CategoriaCatalogo)) e.Add("CategoriaCatalogo es requerida.");
        if (dto.PrecioBase < 0) e.Add("PrecioBase no puede ser negativo.");
        return e;
    }

    public static List<string> ValidarActualizacion(CatalogoServicioUpdateDTO dto)
    {
        var e = new List<string>();
        if (string.IsNullOrWhiteSpace(dto.NombreCatalogo)) e.Add("NombreCatalogo es requerido.");
        if (!TiposValidos.Contains(dto.TipoCatalogo)) e.Add("TipoCatalogo debe ser AME o SRV.");
        if (dto.PrecioBase < 0) e.Add("PrecioBase no puede ser negativo.");
        return e;
    }
}
