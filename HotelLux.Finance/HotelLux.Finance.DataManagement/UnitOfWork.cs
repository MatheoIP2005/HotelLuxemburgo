using HotelLux.Finance.DataAccess.Context;
using HotelLux.Finance.DataAccess.Repositories.Interfaces;
using HotelLux.Finance.DataManagement.Interfaces;

namespace HotelLux.Finance.DataManagement;

public class UnitOfWork : IUnitOfWork
{
    private readonly FinanceDbContext _context;
    public IFacturaRepository FacturaRepository { get; }
    public IPagoRepository PagoRepository { get; }

    public UnitOfWork(FinanceDbContext context, IFacturaRepository facturaRepository, IPagoRepository pagoRepository)
    {
        _context = context;
        FacturaRepository = facturaRepository;
        PagoRepository = pagoRepository;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);
    public void Dispose() => _context.Dispose();
    public ValueTask DisposeAsync() => _context.DisposeAsync();
}
