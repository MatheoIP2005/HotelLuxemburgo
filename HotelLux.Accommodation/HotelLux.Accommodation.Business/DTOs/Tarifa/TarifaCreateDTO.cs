namespace HotelLux.Accommodation.Business.DTOs.Tarifa;

public class TarifaCreateDTO
{
    public string CodigoTarifa { get; set; } = null!;
    public Guid SucursalGuid { get; set; }
    public Guid TipoHabitacionGuid { get; set; }
    public string NombreTarifa { get; set; } = null!;
    public string? CanalTarifa { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public decimal PrecioPorNoche { get; set; }
    public decimal? PorcentajeIva { get; set; }
    public int MinNoches { get; set; }
    public int? MaxNoches { get; set; }
    public bool PermitePortalPublico { get; set; } = true;
    public int? Prioridad { get; set; }
    public string? CreadoPorUsuario { get; set; }
    public string? CreadoDesdeIp { get; set; }
}
