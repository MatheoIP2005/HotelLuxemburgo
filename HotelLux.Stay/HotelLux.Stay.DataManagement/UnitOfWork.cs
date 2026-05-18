using HotelLux.Stay.DataAccess.Context;
using HotelLux.Stay.DataAccess.Repositories.Interfaces;
using HotelLux.Stay.DataManagement.Interfaces;

namespace HotelLux.Stay.DataManagement;

public class UnitOfWork : IUnitOfWork
{
    private readonly StayDbContext _context;
    public IEstadiaRepository EstadiaRepository { get; }
    public IValoracionRepository ValoracionRepository { get; }
    public ICargoEstadiaRepository CargoEstadiaRepository { get; }

    public UnitOfWork(
        StayDbContext context,
        IEstadiaRepository estadiaRepository,
        IValoracionRepository valoracionRepository,
        ICargoEstadiaRepository cargoEstadiaRepository)
    {
        _context = context;
        EstadiaRepository = estadiaRepository;
        ValoracionRepository = valoracionRepository;
        CargoEstadiaRepository = cargoEstadiaRepository;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);
    public void Dispose() => _context.Dispose();
    public ValueTask DisposeAsync() => _context.DisposeAsync();
}
