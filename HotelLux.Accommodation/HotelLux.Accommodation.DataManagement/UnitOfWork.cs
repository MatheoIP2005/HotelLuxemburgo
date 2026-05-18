using HotelLux.Accommodation.DataAccess.Context;
using HotelLux.Accommodation.DataAccess.Repositories.Interfaces;
using HotelLux.Accommodation.DataManagement.Interfaces;

namespace HotelLux.Accommodation.DataManagement.Services;

public class UnitOfWork : IUnitOfWork
{
    private readonly AccommodationDbContext _context;

    public ISucursalRepository SucursalRepository { get; }
    public ISucursalImagenRepository SucursalImagenRepository { get; }
    public ITipoHabitacionRepository TipoHabitacionRepository { get; }
    public ITipoHabitacionImagenRepository TipoHabitacionImagenRepository { get; }
    public IHabitacionRepository HabitacionRepository { get; }
    public ITarifaRepository TarifaRepository { get; }
    public ICatalogoServicioRepository CatalogoServicioRepository { get; }
    public ITipoHabitacionCatalogoRepository TipoHabitacionCatalogoRepository { get; }

    public UnitOfWork(
        AccommodationDbContext context,
        ISucursalRepository sucursalRepository,
        ISucursalImagenRepository sucursalImagenRepository,
        ITipoHabitacionRepository tipoHabitacionRepository,
        ITipoHabitacionImagenRepository tipoHabitacionImagenRepository,
        IHabitacionRepository habitacionRepository,
        ITarifaRepository tarifaRepository,
        ICatalogoServicioRepository catalogoServicioRepository,
        ITipoHabitacionCatalogoRepository tipoHabitacionCatalogoRepository)
    {
        _context = context;
        SucursalRepository = sucursalRepository;
        SucursalImagenRepository = sucursalImagenRepository;
        TipoHabitacionRepository = tipoHabitacionRepository;
        TipoHabitacionImagenRepository = tipoHabitacionImagenRepository;
        HabitacionRepository = habitacionRepository;
        TarifaRepository = tarifaRepository;
        CatalogoServicioRepository = catalogoServicioRepository;
        TipoHabitacionCatalogoRepository = tipoHabitacionCatalogoRepository;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
