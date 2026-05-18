using HotelLux.Auth.DataAccess.Context;
using HotelLux.Auth.DataAccess.Repositories.Interfaces;
using HotelLux.Auth.DataManagement.Interfaces;

namespace HotelLux.Auth.DataManagement.Services;

public class UnitOfWork : IUnitOfWork
{
    private readonly AuthDbContext _dbContext;

    public IUsuarioAppRepository UsuarioAppRepository { get; }
    public IRolRepository RolRepository { get; }

    public UnitOfWork(AuthDbContext dbContext, IUsuarioAppRepository usuarioAppRepository, IRolRepository rolRepository)
    {
        _dbContext = dbContext;
        UsuarioAppRepository = usuarioAppRepository;
        RolRepository = rolRepository;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
