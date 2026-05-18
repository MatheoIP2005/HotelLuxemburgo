using HotelLux.Accommodation.DataManagement.Interfaces;
using HotelLux.Accommodation.DataManagement.Mappers;
using HotelLux.Accommodation.DataManagement.Models;

namespace HotelLux.Accommodation.DataManagement.Services;

public class TipoHabitacionImagenDataService : ITipoHabitacionImagenDataService
{
    private readonly IUnitOfWork _uow;

    public TipoHabitacionImagenDataService(IUnitOfWork uow) => _uow = uow;

    public async Task<IReadOnlyList<TipoHabitacionImagenDataModel>> ListarPorTipoAsync(Guid tipoGuid, CancellationToken ct = default)
    {
        var t = await _uow.TipoHabitacionRepository.ObtenerPorGuidAsync(tipoGuid, ct);
        if (t is null) return Array.Empty<TipoHabitacionImagenDataModel>();
        var list = await _uow.TipoHabitacionImagenRepository.ListarPorTipoAsync(t.IdTipoHabitacion, ct);
        return list.Select(TipoHabitacionImagenDataMapper.ToDataModel).ToList();
    }

    public async Task<TipoHabitacionImagenDataModel> CrearAsync(TipoHabitacionImagenDataModel model, CancellationToken ct = default)
    {
        var entity = TipoHabitacionImagenDataMapper.ToEntity(model);
        await _uow.TipoHabitacionImagenRepository.AgregarAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        var reloaded = await _uow.TipoHabitacionImagenRepository.ObtenerPorIdAsync(entity.IdTipoHabitacionImagen, ct)
            ?? entity;
        return TipoHabitacionImagenDataMapper.ToDataModel(reloaded);
    }

    public async Task EliminarAsync(Guid tipoGuid, int idImagen, CancellationToken ct = default)
    {
        var t = await _uow.TipoHabitacionRepository.ObtenerPorGuidAsync(tipoGuid, ct);
        if (t is null) return;
        var img = await _uow.TipoHabitacionImagenRepository.ObtenerPorIdAsync(idImagen, ct);
        if (img is null || img.IdTipoHabitacion != t.IdTipoHabitacion) return;
        _uow.TipoHabitacionImagenRepository.Eliminar(img);
        await _uow.SaveChangesAsync(ct);
    }
}
