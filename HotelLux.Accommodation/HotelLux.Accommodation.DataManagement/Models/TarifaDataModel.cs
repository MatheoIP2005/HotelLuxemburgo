namespace HotelLux.Accommodation.DataManagement.Models;

public class TarifaDataModel
{
    public int IdTarifa { get; set; }
    public Guid TarifaGuid { get; set; }
    public string CodigoTarifa { get; set; } = null!;
    public int IdSucursal { get; set; }
    public int IdTipoHabitacion { get; set; }
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
    public bool EsEliminado { get; set; }
    public DateTimeOffset? FechaInhabilitacionUtc { get; set; }
    public string? MotivoInhabilitacion { get; set; }
    public DateTimeOffset FechaRegistroUtc { get; set; }
    public string CreadoPorUsuario { get; set; } = null!;
    public string? ModificadoPorUsuario { get; set; }
    public DateTimeOffset? FechaModificacionUtc { get; set; }
    public string? ModificacionIp { get; set; }
    public string ServicioOrigen { get; set; } = null!;
}
