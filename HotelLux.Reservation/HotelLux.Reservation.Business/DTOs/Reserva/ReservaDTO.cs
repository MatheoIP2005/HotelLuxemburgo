using HotelLux.Reservation.Business.DTOs.ReservaHabitacion;

namespace HotelLux.Reservation.Business.DTOs.Reserva;

public class ReservaDTO
{
    public Guid ReservaGuid { get; set; }
    public string CodigoReserva { get; set; } = null!;
    public Guid ClienteGuid { get; set; }
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
    public DateTimeOffset FechaRegistroUtc { get; set; }
    public string CreadoPorUsuario { get; set; } = null!;
    public IReadOnlyList<ReservaHabitacionDTO> Habitaciones { get; set; } = new List<ReservaHabitacionDTO>();
}
