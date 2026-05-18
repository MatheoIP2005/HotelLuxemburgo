namespace HotelLux.Accommodation.DataAccess.Entities;

public class TipoHabitacionCatalogoEntity
{
    public int IdTipoHabCatalogo { get; set; }
    public int IdTipoHabitacion { get; set; }
    public int IdCatalogo { get; set; }
    public DateTimeOffset FechaRegistroUtc { get; set; }
    public string CreadoPorUsuario { get; set; } = null!;

    public TipoHabitacionEntity TipoHabitacion { get; set; } = null!;
    public CatalogoServicioEntity CatalogoServicio { get; set; } = null!;
}
