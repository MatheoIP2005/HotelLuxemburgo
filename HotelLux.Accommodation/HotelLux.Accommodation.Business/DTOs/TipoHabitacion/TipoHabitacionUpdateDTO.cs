namespace HotelLux.Accommodation.Business.DTOs.TipoHabitacion;

public class TipoHabitacionUpdateDTO
{
    public string CodigoTipoHabitacion { get; set; } = null!;
    public string NombreTipoHabitacion { get; set; } = null!;
    public string? Descripcion { get; set; }
    public int CapacidadAdultos { get; set; }
    public int CapacidadNinos { get; set; }
    public int CapacidadTotal { get; set; }
    public string? TipoCama { get; set; }
    public decimal? AreaM2 { get; set; }
    public bool PermiteEventos { get; set; }
    public bool PermiteReservaPublica { get; set; }
    public string EstadoTipoHabitacion { get; set; } = null!;
    public string? ModificadoPorUsuario { get; set; }
    public string? ModificadoDesdeIp { get; set; }
}
