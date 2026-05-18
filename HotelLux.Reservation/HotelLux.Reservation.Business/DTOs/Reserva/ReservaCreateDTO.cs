using HotelLux.Reservation.Business.DTOs.ReservaHabitacion;

namespace HotelLux.Reservation.Business.DTOs.Reserva;

public class ReservaCreateDTO
{
    public Guid? ClienteGuid { get; set; }
    public ClienteInlineDTO? Cliente { get; set; }
    public Guid SucursalGuid { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public decimal SubtotalReserva { get; set; }
    public decimal ValorIva { get; set; }
    public decimal TotalReserva { get; set; }
    public decimal DescuentoAplicado { get; set; }
    public decimal SaldoPendiente { get; set; }
    public string OrigenCanalReserva { get; set; } = null!;
    public string? Observaciones { get; set; }
    public bool EsWalkin { get; set; }
    public List<ReservaHabitacionCreateDTO> Habitaciones { get; set; } = new();
    public string? CreadoPorUsuario { get; set; }
    public string? CreadoDesdeIp { get; set; }

    /// <summary>Campos de extensión del contrato público (endpoints_publicas.txt).</summary>
    public string? AdditionalProp1 { get; set; }
    public string? AdditionalProp2 { get; set; }
    public string? AdditionalProp3 { get; set; }
}
