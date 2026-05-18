namespace HotelLux.Stay.Business.Interfaces;

public sealed class ReservaHabitacionValidada
{
    public Guid ReservaHabitacionGuid { get; init; }
    public Guid HabitacionGuid { get; init; }
    public DateOnly FechaInicio { get; init; }
    public DateOnly FechaFin { get; init; }
}

public sealed class ValidacionCheckinResult
{
    public bool Valid { get; init; }
    public string Mensaje { get; init; } = string.Empty;
    public Guid ClienteGuid { get; init; }
    public Guid SucursalGuid { get; init; }
    public IReadOnlyList<ReservaHabitacionValidada> Lineas { get; init; } = Array.Empty<ReservaHabitacionValidada>();
}

public interface IReservationStayClient
{
    Task<ValidacionCheckinResult> ValidarReservaParaCheckinAsync(Guid reservaGuid, CancellationToken ct = default);
}
