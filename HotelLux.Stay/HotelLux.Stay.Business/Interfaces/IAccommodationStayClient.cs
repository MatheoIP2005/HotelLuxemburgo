namespace HotelLux.Stay.Business.Interfaces;

public interface IAccommodationStayClient
{
    Task<bool> UpdateRoomStatusAsync(Guid habitacionGuid, string nuevoEstado, Guid operacionGuid, CancellationToken ct = default);
}
