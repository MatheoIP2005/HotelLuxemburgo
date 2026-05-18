namespace HotelLux.Reservation.Business.DTOs.Stay;

public sealed class StayValoracionClienteDto
{
    public Guid ValoracionGuid { get; set; }
    public Guid ClienteGuid { get; set; }
    public double PuntuacionGeneral { get; set; }
    public string ComentarioPositivo { get; set; } = "";
    public string ComentarioNegativo { get; set; } = "";
    public string TipoViaje { get; set; } = "";
    public string FechaPublicacion { get; set; } = "";
    public string RespuestaHotel { get; set; } = "";
    public string NombreVisibleCliente { get; set; } = "";
}
