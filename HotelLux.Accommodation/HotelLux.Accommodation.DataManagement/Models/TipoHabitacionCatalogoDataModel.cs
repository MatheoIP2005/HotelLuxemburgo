namespace HotelLux.Accommodation.DataManagement.Models;

public class TipoHabitacionCatalogoDataModel
{
    public int IdTipoHabCatalogo { get; set; }
    public int IdTipoHabitacion { get; set; }
    public int IdCatalogo { get; set; }
    public DateTimeOffset FechaRegistroUtc { get; set; }
    public string CreadoPorUsuario { get; set; } = null!;
}
