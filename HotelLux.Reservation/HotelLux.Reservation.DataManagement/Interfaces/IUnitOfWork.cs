using HotelLux.Reservation.DataAccess.Repositories.Interfaces;

namespace HotelLux.Reservation.DataManagement.Interfaces;

public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    IReservaRepository ReservaRepository { get; }
    IReservaHabitacionRepository ReservaHabitacionRepository { get; }
    IClienteRepository ClienteRepository { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
