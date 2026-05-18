using HotelLux.Finance.DataAccess.Repositories.Interfaces;

namespace HotelLux.Finance.DataManagement.Interfaces;

public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    IFacturaRepository FacturaRepository { get; }
    IPagoRepository PagoRepository { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
