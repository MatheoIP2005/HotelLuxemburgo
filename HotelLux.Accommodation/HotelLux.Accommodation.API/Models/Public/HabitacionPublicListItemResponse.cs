namespace HotelLux.Accommodation.API.Models.Public;

/// <summary>Forma canónica de ítem en listados públicos de habitaciones (endpoints_publicas.txt y HabitacionPublicListItemDto en endpoints_locales.txt).</summary>
public sealed class HabitacionPublicListItemResponse
{
    public Guid HabitacionGuid { get; init; }
    public Guid TipoHabitacionGuid { get; init; }
    public string? TipoNombre { get; init; }
    public string NumeroHabitacion { get; init; } = "";
    public int Piso { get; init; }
    public int CapacidadAdultos { get; init; }
    public int CapacidadNinos { get; init; }
    public decimal PrecioBase { get; init; }
    public string Moneda { get; init; } = "USD";
    public string EstadoHabitacion { get; init; } = "";
    public bool DisponibleEnRango { get; init; }
}
