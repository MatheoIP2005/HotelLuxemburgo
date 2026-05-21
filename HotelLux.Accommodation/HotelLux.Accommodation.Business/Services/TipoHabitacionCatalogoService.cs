using HotelLux.Accommodation.Business.DTOs.CatalogoServicio;
using HotelLux.Accommodation.Business.Exceptions;
using HotelLux.Accommodation.Business.Interfaces;
using HotelLux.Accommodation.Business.Mappers;
using HotelLux.Accommodation.DataManagement.Interfaces;

namespace HotelLux.Accommodation.Business.Services;

public class TipoHabitacionCatalogoService : ITipoHabitacionCatalogoService
{
    private readonly ITipoHabitacionCatalogoDataService _dataService;
    private readonly ITipoHabitacionDataService _tipoDataService;
    private readonly ICatalogoServicioDataService _catalogoDataService;

    public TipoHabitacionCatalogoService(
        ITipoHabitacionCatalogoDataService dataService,
        ITipoHabitacionDataService tipoDataService,
        ICatalogoServicioDataService catalogoDataService)
    {
        _dataService = dataService;
        _tipoDataService = tipoDataService;
        _catalogoDataService = catalogoDataService;
    }

    public async Task<IReadOnlyList<CatalogoServicioDTO>> ListarPorTipoAsync(Guid tipoGuid, CancellationToken ct = default)
    {
        var tipo = await _tipoDataService.ObtenerPorGuidAsync(tipoGuid, ct);
        if (tipo is null) throw new NotFoundException("TipoHabitación", tipoGuid);
        var models = await _dataService.ListarPorTipoAsync(tipo.IdTipoHabitacion, ct);
        return models.Select(CatalogoServicioBusinessMapper.ToDTO).ToList();
    }

    public async Task AsignarAsync(Guid tipoGuid, Guid catalogoGuid, string usuario, CancellationToken ct = default)
    {
        var tipo = await _tipoDataService.ObtenerPorGuidAsync(tipoGuid, ct);
        if (tipo is null) throw new NotFoundException("TipoHabitación", tipoGuid);

        var catalogo = await _catalogoDataService.ObtenerPorGuidAsync(catalogoGuid, ct);
        if (catalogo is null) throw new NotFoundException("CatálogoServicio", catalogoGuid);

        var existente = await _dataService.ObtenerAsync(tipo.IdTipoHabitacion, catalogo.IdCatalogo, ct);
        if (existente is not null)
            throw new ConflictException("El servicio ya está asignado a este tipo de habitación.");

        await _dataService.AsignarAsync(tipo.IdTipoHabitacion, catalogo.IdCatalogo, usuario, ct);
    }

    public async Task RemoverAsync(Guid tipoGuid, Guid catalogoGuid, CancellationToken ct = default)
    {
        var tipo = await _tipoDataService.ObtenerPorGuidAsync(tipoGuid, ct);
        if (tipo is null) throw new NotFoundException("TipoHabitación", tipoGuid);

        var catalogo = await _catalogoDataService.ObtenerPorGuidAsync(catalogoGuid, ct);
        if (catalogo is null) throw new NotFoundException("CatálogoServicio", catalogoGuid);

        await _dataService.RemoverAsync(tipo.IdTipoHabitacion, catalogo.IdCatalogo, ct);
    }

    public async Task RemoverPorIdAsync(Guid tipoGuid, int idTipoHabCatalogo, CancellationToken ct = default)
    {
        var tipo = await _tipoDataService.ObtenerPorGuidAsync(tipoGuid, ct);
        if (tipo is null) throw new NotFoundException("TipoHabitación", tipoGuid);

        await _dataService.RemoverPorIdAsync(tipo.IdTipoHabitacion, idTipoHabCatalogo, ct);
    }
}
