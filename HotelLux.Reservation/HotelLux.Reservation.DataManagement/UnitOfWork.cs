using HotelLux.Reservation.DataAccess.Context;
using HotelLux.Reservation.DataAccess.Repositories.Interfaces;
using HotelLux.Reservation.DataManagement.Interfaces;

namespace HotelLux.Reservation.DataManagement;

public class UnitOfWork : IUnitOfWork
{
    private readonly ReservationDbContext _context;

    public IReservaRepository ReservaRepository { get; }
    public IReservaHabitacionRepository ReservaHabitacionRepository { get; }
    public IClienteRepository ClienteRepository { get; }

    public UnitOfWork(
        ReservationDbContext context,
        IReservaRepository reservaRepository,
        IReservaHabitacionRepository reservaHabitacionRepository,
        IClienteRepository clienteRepository)
    {
        _context = context;
        ReservaRepository = reservaRepository;
        ReservaHabitacionRepository = reservaHabitacionRepository;
        ClienteRepository = clienteRepository;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);

    public void Dispose() => _context.Dispose();
    public ValueTask DisposeAsync() => _context.DisposeAsync();
}
