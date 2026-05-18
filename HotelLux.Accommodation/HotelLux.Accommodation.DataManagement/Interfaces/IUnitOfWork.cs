using HotelLux.Accommodation.DataAccess.Repositories.Interfaces;

namespace HotelLux.Accommodation.DataManagement.Interfaces;

public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    ISucursalRepository SucursalRepository { get; }
    ISucursalImagenRepository SucursalImagenRepository { get; }
    ITipoHabitacionRepository TipoHabitacionRepository { get; }
    ITipoHabitacionImagenRepository TipoHabitacionImagenRepository { get; }
    IHabitacionRepository HabitacionRepository { get; }
    ITarifaRepository TarifaRepository { get; }
    ICatalogoServicioRepository CatalogoServicioRepository { get; }
    ITipoHabitacionCatalogoRepository TipoHabitacionCatalogoRepository { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
