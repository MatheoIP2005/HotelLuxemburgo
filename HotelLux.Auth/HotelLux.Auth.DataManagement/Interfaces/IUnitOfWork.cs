using HotelLux.Auth.DataAccess.Repositories.Interfaces;

namespace HotelLux.Auth.DataManagement.Interfaces;

public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    IUsuarioAppRepository UsuarioAppRepository { get; }
    IRolRepository RolRepository { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
