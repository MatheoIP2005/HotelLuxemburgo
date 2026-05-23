namespace HotelLux.Reservation.Business.Interfaces;

public sealed record HabitacionDisponibleInfo(
    Guid HabitacionGuid,
    Guid TipoHabitacionGuid,
    decimal PrecioNoche);

public interface IAccommodationClient
{
    /// <summary>Habitaciones físicas disponibles en un rango, opcionalmente filtradas por tipo.</summary>
    Task<IReadOnlyList<HabitacionDisponibleInfo>> ListarDisponiblesAsync(
        Guid sucursalGuid,
        DateOnly fechaInicio,
        DateOnly fechaFin,
        Guid? tipoHabitacionGuid = null,
        int cantidadPersonas = 0,
        CancellationToken ct = default);

    /// <summary>Resuelve la primera habitación disponible de un tipo con tarifa activa.</summary>
    Task<HabitacionDisponibleInfo?> ResolverPorTipoAsync(
        Guid sucursalGuid,
        Guid tipoHabitacionGuid,
        DateOnly fechaInicio,
        DateOnly fechaFin,
        CancellationToken ct = default);

    /// <summary>Bloquea la habitación (DIS→OCU). Requiere fechas para el contrato gRPC.</summary>
    Task<bool> ConfirmRoomLockAsync(
        Guid habitacionGuid, Guid reservaGuid,
        DateOnly fechaInicio, DateOnly fechaFin,
        CancellationToken ct = default);

    Task<bool> ReleaseRoomLockAsync(Guid habitacionGuid, Guid reservaGuid, CancellationToken ct = default);
}
