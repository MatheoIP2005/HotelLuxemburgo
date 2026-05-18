using HotelLux.Accommodation.DataAccess.Entities;
using HotelLux.Accommodation.DataManagement.Interfaces;
using HotelLux.Accommodation.DataManagement.Mappers;
using HotelLux.Accommodation.DataManagement.Models;

namespace HotelLux.Accommodation.DataManagement.Services;

public class TipoHabitacionCatalogoDataService : ITipoHabitacionCatalogoDataService
{
    private readonly IUnitOfWork _uow;

    public TipoHabitacionCatalogoDataService(IUnitOfWork uow) => _uow = uow;

    public async Task<IReadOnlyList<CatalogoServicioDataModel>> ListarPorTipoAsync(int idTipoHabitacion, CancellationToken ct = default)
    {
        var rel = await _uow.TipoHabitacionCatalogoRepository.ListarPorTipoAsync(idTipoHabitacion, ct);
        return rel
            .Where(x => !x.CatalogoServicio.EsEliminado)
            .Select(x => CatalogoServicioDataMapper.ToDataModel(x.CatalogoServicio))
            .ToList();
    }

    public async Task<TipoHabitacionCatalogoDataModel?> ObtenerAsync(int idTipo, int idCatalogo, CancellationToken ct = default)
    {
        var e = await _uow.TipoHabitacionCatalogoRepository.ObtenerAsync(idTipo, idCatalogo, ct);
        return e is null ? null : TipoHabitacionCatalogoDataMapper.ToDataModel(e);
    }

    public async Task AsignarAsync(int idTipoHabitacion, int idCatalogo, string usuario, CancellationToken ct = default)
    {
        var existing = await _uow.TipoHabitacionCatalogoRepository.ObtenerAsync(idTipoHabitacion, idCatalogo, ct);
        if (existing is not null) return;

        var entity = new TipoHabitacionCatalogoEntity
        {
            IdTipoHabitacion = idTipoHabitacion,
            IdCatalogo = idCatalogo,
            FechaRegistroUtc = DateTimeOffset.UtcNow,
            CreadoPorUsuario = usuario
        };
        await _uow.TipoHabitacionCatalogoRepository.AgregarAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task RemoverAsync(int idTipoHabitacion, int idCatalogo, CancellationToken ct = default)
    {
        var entity = await _uow.TipoHabitacionCatalogoRepository.ObtenerAsync(idTipoHabitacion, idCatalogo, ct);
        if (entity is null) return;
        _uow.TipoHabitacionCatalogoRepository.Eliminar(entity);
        await _uow.SaveChangesAsync(ct);
    }
}
