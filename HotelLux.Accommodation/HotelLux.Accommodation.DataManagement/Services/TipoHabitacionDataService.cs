using HotelLux.Accommodation.DataManagement.Interfaces;
using HotelLux.Accommodation.DataManagement.Mappers;
using HotelLux.Accommodation.DataManagement.Models;

namespace HotelLux.Accommodation.DataManagement.Services;

public class TipoHabitacionDataService : ITipoHabitacionDataService
{
    private readonly IUnitOfWork _uow;

    public TipoHabitacionDataService(IUnitOfWork uow) => _uow = uow;

    public async Task<TipoHabitacionDataModel?> ObtenerPorIdAsync(int idTipoHabitacion, CancellationToken ct = default)
    {
        var e = await _uow.TipoHabitacionRepository.ObtenerPorIdAsync(idTipoHabitacion, ct);
        return e is null ? null : TipoHabitacionDataMapper.ToDataModel(e);
    }

    public async Task<TipoHabitacionDataModel?> ObtenerPorGuidAsync(Guid guid, CancellationToken ct = default)
    {
        var e = await _uow.TipoHabitacionRepository.ObtenerPorGuidAsync(guid, ct);
        return e is null ? null : TipoHabitacionDataMapper.ToDataModel(e);
    }

    public async Task<IReadOnlyList<TipoHabitacionDataModel>> ListarAsync(CancellationToken ct = default)
    {
        var list = await _uow.TipoHabitacionRepository.ListarAsync(ct);
        return list.Select(TipoHabitacionDataMapper.ToDataModel).ToList();
    }

    public async Task<TipoHabitacionDataModel> CrearAsync(TipoHabitacionDataModel model, CancellationToken ct = default)
    {
        var entity = TipoHabitacionDataMapper.ToEntity(model);
        if (entity.TipoHabitacionGuid == Guid.Empty)
            entity.TipoHabitacionGuid = Guid.NewGuid();
        await _uow.TipoHabitacionRepository.AgregarAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        var reloaded = await _uow.TipoHabitacionRepository.ObtenerPorIdAsync(entity.IdTipoHabitacion, ct) ?? entity;
        return TipoHabitacionDataMapper.ToDataModel(reloaded);
    }

    public async Task<TipoHabitacionDataModel?> ActualizarAsync(TipoHabitacionDataModel model, CancellationToken ct = default)
    {
        var exists = await _uow.TipoHabitacionRepository.ObtenerPorIdAsync(model.IdTipoHabitacion, ct);
        if (exists is null) return null;
        var incoming = TipoHabitacionDataMapper.ToEntity(model);
        incoming.TipoHabitacionGuid = exists.TipoHabitacionGuid;
        incoming.FechaRegistroUtc = exists.FechaRegistroUtc;
        incoming.CreadoPorUsuario = exists.CreadoPorUsuario;
        _uow.TipoHabitacionRepository.Actualizar(incoming);
        await _uow.SaveChangesAsync(ct);
        var reloaded = await _uow.TipoHabitacionRepository.ObtenerPorIdAsync(model.IdTipoHabitacion, ct);
        return reloaded is null ? null : TipoHabitacionDataMapper.ToDataModel(reloaded);
    }

    public async Task<bool> EliminarLogicoAsync(int idTipoHabitacion, string usuario, CancellationToken ct = default)
    {
        var e = await _uow.TipoHabitacionRepository.ObtenerParaActualizarAsync(idTipoHabitacion, ct);
        if (e is null) return false;
        e.EsEliminado = true;
        e.ModificadoPorUsuario = usuario;
        e.FechaModificacionUtc = DateTimeOffset.UtcNow;
        _uow.TipoHabitacionRepository.Actualizar(e);
        await _uow.SaveChangesAsync(ct);
        return true;
    }
}
