namespace HotelLux.Reservation.Business.DTOs.Reserva;

public class ClienteInlineDTO
{
    public string TipoIdentificacion { get; set; } = null!;
    public string NumeroIdentificacion { get; set; } = null!;
    public string Nombres { get; set; } = null!;
    public string Apellidos { get; set; } = null!;
    public string Correo { get; set; } = null!;
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
}
