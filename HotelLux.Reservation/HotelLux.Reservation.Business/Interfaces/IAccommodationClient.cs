namespace HotelLux.Reservation.Business.Interfaces;

public interface IAccommodationClient
{
    /// <summary>Bloquea la habitación (DIS→OCU). Requiere fechas para el contrato gRPC.</summary>
    Task<bool> ConfirmRoomLockAsync(
        Guid habitacionGuid, Guid reservaGuid,
        DateOnly fechaInicio, DateOnly fechaFin,
        CancellationToken ct = default);

    Task<bool> ReleaseRoomLockAsync(Guid habitacionGuid, Guid reservaGuid, CancellationToken ct = default);
}
