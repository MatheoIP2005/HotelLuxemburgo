using HotelLux.Accommodation.DataManagement.Interfaces;
using HotelLux.Accommodation.DataManagement.Mappers;
using HotelLux.Accommodation.DataManagement.Models;

namespace HotelLux.Accommodation.DataManagement.Services;

public class TarifaDataService : ITarifaDataService
{
    private readonly IUnitOfWork _uow;

    public TarifaDataService(IUnitOfWork uow) => _uow = uow;

    public async Task<TarifaDataModel?> ObtenerPorIdAsync(int idTarifa, CancellationToken ct = default)
    {
        var e = await _uow.TarifaRepository.ObtenerPorIdAsync(idTarifa, ct);
        return e is null ? null : TarifaDataMapper.ToDataModel(e);
    }

    public async Task<TarifaDataModel?> ObtenerPorGuidAsync(Guid guid, CancellationToken ct = default)
    {
        var e = await _uow.TarifaRepository.ObtenerPorGuidAsync(guid, ct);
        return e is null ? null : TarifaDataMapper.ToDataModel(e);
    }

    public async Task<IReadOnlyList<TarifaDataModel>> ListarAsync(CancellationToken ct = default)
    {
        var list = await _uow.TarifaRepository.ListarAsync(ct);
        return list.Select(TarifaDataMapper.ToDataModel).ToList();
    }

    public async Task<IReadOnlyList<TarifaDataModel>> ListarPorSucursalAsync(Guid sucursalGuid, CancellationToken ct = default)
    {
        var s = await _uow.SucursalRepository.ObtenerPorGuidAsync(sucursalGuid, ct);
        if (s is null) return Array.Empty<TarifaDataModel>();
        var list = await _uow.TarifaRepository.ListarPorSucursalAsync(s.IdSucursal, ct);
        return list.Select(TarifaDataMapper.ToDataModel).ToList();
    }

    public async Task<TarifaDataModel> CrearAsync(TarifaDataModel model, CancellationToken ct = default)
    {
        var entity = TarifaDataMapper.ToEntity(model);
        if (entity.TarifaGuid == Guid.Empty)
            entity.TarifaGuid = Guid.NewGuid();
        await _uow.TarifaRepository.AgregarAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        var reloaded = await _uow.TarifaRepository.ObtenerPorIdAsync(entity.IdTarifa, ct) ?? entity;
        return TarifaDataMapper.ToDataModel(reloaded);
    }

    public async Task<TarifaDataModel?> ActualizarAsync(TarifaDataModel model, CancellationToken ct = default)
    {
        var exists = await _uow.TarifaRepository.ObtenerPorIdAsync(model.IdTarifa, ct);
        if (exists is null) return null;
        var incoming = TarifaDataMapper.ToEntity(model);
        incoming.TarifaGuid = exists.TarifaGuid;
        incoming.FechaRegistroUtc = exists.FechaRegistroUtc;
        incoming.CreadoPorUsuario = exists.CreadoPorUsuario;
        incoming.IdSucursal = exists.IdSucursal;
        incoming.IdTipoHabitacion = exists.IdTipoHabitacion;
        _uow.TarifaRepository.Actualizar(incoming);
        await _uow.SaveChangesAsync(ct);
        var reloaded = await _uow.TarifaRepository.ObtenerPorIdAsync(model.IdTarifa, ct);
        return reloaded is null ? null : TarifaDataMapper.ToDataModel(reloaded);
    }

    public async Task DesactivarAsync(Guid tarifaGuid, string usuario, CancellationToken ct = default)
    {
        var e = await _uow.TarifaRepository.ObtenerParaActualizarPorGuidAsync(tarifaGuid, ct);
        if (e is null) return;
        e.EstadoTarifa = "INA";
        e.FechaInhabilitacionUtc = DateTimeOffset.UtcNow;
        e.MotivoInhabilitacion = $"Desactivada por {usuario}";
        e.ModificadoPorUsuario = usuario;
        e.FechaModificacionUtc = DateTimeOffset.UtcNow;
        _uow.TarifaRepository.Actualizar(e);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<bool> EliminarLogicoAsync(int idTarifa, string usuario, CancellationToken ct = default)
    {
        var e = await _uow.TarifaRepository.ObtenerParaActualizarAsync(idTarifa, ct);
        if (e is null) return false;
        e.EsEliminado = true;
        e.ModificadoPorUsuario = usuario;
        e.FechaModificacionUtc = DateTimeOffset.UtcNow;
        _uow.TarifaRepository.Actualizar(e);
        await _uow.SaveChangesAsync(ct);
        return true;
    }
}
