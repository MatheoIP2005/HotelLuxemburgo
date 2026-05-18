namespace HotelLux.Reservation.DataAccess.Entities;

public class ReservaEntity
{
    public int IdReserva { get; set; }
    public Guid ReservaGuid { get; set; }
    public string CodigoReserva { get; set; } = null!;

    public int IdCliente { get; set; }
    public Guid ClienteGuid { get; set; }
    public ClienteEntity? Cliente { get; set; }
    public Guid SucursalGuid { get; set; }

    public DateTimeOffset FechaReservaUtc { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }

    public decimal SubtotalReserva { get; set; }
    public decimal ValorIva { get; set; }
    public decimal TotalReserva { get; set; }
    public decimal DescuentoAplicado { get; set; }
    public decimal SaldoPendiente { get; set; }

    public string OrigenCanalReserva { get; set; } = null!;
    public string EstadoReserva { get; set; } = null!;

    public DateTimeOffset? FechaConfirmacionUtc { get; set; }
    public DateTimeOffset? FechaCancelacionUtc { get; set; }
    public string? MotivoCancelacion { get; set; }
    public string? Observaciones { get; set; }
    public bool EsWalkin { get; set; }

    public bool EsEliminado { get; set; }
    public DateTimeOffset? FechaInhabilitacionUtc { get; set; }
    public string? MotivoInhabilitacion { get; set; }

    public DateTimeOffset FechaRegistroUtc { get; set; }
    public string CreadoPorUsuario { get; set; } = null!;
    public string? CreadoDesdeIp { get; set; }
    public string? ModificadoPorUsuario { get; set; }
    public DateTimeOffset? FechaModificacionUtc { get; set; }
    public string? ModificacionIp { get; set; }
    public string ServicioOrigen { get; set; } = null!;

    public ICollection<ReservaHabitacionEntity> ReservasHabitaciones { get; set; } = new List<ReservaHabitacionEntity>();
}
