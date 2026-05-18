using HotelLux.Stay.DataAccess.Repositories.Interfaces;

namespace HotelLux.Stay.DataManagement.Interfaces;

public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    IEstadiaRepository EstadiaRepository { get; }
    IValoracionRepository ValoracionRepository { get; }
    ICargoEstadiaRepository CargoEstadiaRepository { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
