namespace HotelLux.Accommodation.Business.DTOs.Tarifa;

public class TarifaDTO
{
    public Guid TarifaGuid { get; set; }
    public string CodigoTarifa { get; set; } = null!;
    public Guid SucursalGuid { get; set; }
    public Guid TipoHabitacionGuid { get; set; }
    public string NombreTarifa { get; set; } = null!;
    public string CanalTarifa { get; set; } = null!;
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public decimal PrecioPorNoche { get; set; }
    public decimal PorcentajeIva { get; set; }
    public int MinNoches { get; set; }
    public int? MaxNoches { get; set; }
    public bool PermitePortalPublico { get; set; }
    public int Prioridad { get; set; }
    public string EstadoTarifa { get; set; } = null!;
    public DateTimeOffset FechaRegistroUtc { get; set; }
    public string CreadoPorUsuario { get; set; } = null!;
}
