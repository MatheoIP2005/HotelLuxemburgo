using HotelLux.Reservation.Business.DTOs.Stay;

namespace HotelLux.Reservation.Business.Interfaces;

public interface IStayClient
{
    Task<IReadOnlyList<StayValoracionClienteDto>> GetValoracionesByClienteAsync(Guid clienteGuid, CancellationToken ct = default);
}
