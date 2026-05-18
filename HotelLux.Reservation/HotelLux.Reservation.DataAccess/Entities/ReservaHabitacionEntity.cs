namespace HotelLux.Reservation.DataAccess.Entities;

public class ReservaHabitacionEntity
{
    public int IdReservaHabitacion { get; set; }
    public Guid ReservaHabitacionGuid { get; set; }
    public int IdReserva { get; set; }

    public Guid HabitacionGuid { get; set; }
    public Guid? TarifaGuid { get; set; }

    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }

    public int NumAdultos { get; set; }
    public int NumNinos { get; set; }

    public decimal PrecioNocheAplicado { get; set; }
    public decimal SubtotalLinea { get; set; }
    public decimal ValorIvaLinea { get; set; }
    public decimal DescuentoLinea { get; set; }
    public decimal TotalLinea { get; set; }

    public string EstadoDetalle { get; set; } = null!;

    public DateTimeOffset FechaRegistroUtc { get; set; }
    public string CreadoPorUsuario { get; set; } = null!;
    public string? ModificadoPorUsuario { get; set; }
    public DateTimeOffset? FechaModificacionUtc { get; set; }
    public string? ModificacionIp { get; set; }
    public string ServicioOrigen { get; set; } = null!;

    public ReservaEntity Reserva { get; set; } = null!;
}
