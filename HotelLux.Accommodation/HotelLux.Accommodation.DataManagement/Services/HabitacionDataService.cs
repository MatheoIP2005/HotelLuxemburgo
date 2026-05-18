using HotelLux.Accommodation.DataManagement.Interfaces;
using HotelLux.Accommodation.DataManagement.Mappers;
using HotelLux.Accommodation.DataManagement.Models;

namespace HotelLux.Accommodation.DataManagement.Services;

public class HabitacionDataService : IHabitacionDataService
{
    private readonly IUnitOfWork _uow;

    public HabitacionDataService(IUnitOfWork uow) => _uow = uow;

    public async Task<HabitacionDataModel?> ObtenerPorIdAsync(int idHabitacion, CancellationToken ct = default)
    {
        var e = await _uow.HabitacionRepository.ObtenerPorIdAsync(idHabitacion, ct);
        return e is null ? null : HabitacionDataMapper.ToDataModel(e);
    }

    public async Task<HabitacionDataModel?> ObtenerPorGuidAsync(Guid guid, CancellationToken ct = default)
    {
        var e = await _uow.HabitacionRepository.ObtenerPorGuidAsync(guid, ct);
        return e is null ? null : HabitacionDataMapper.ToDataModel(e);
    }

    public async Task<IReadOnlyList<HabitacionDataModel>> ListarAsync(CancellationToken ct = default)
    {
        var list = await _uow.HabitacionRepository.ListarAsync(ct);
        return list.Select(HabitacionDataMapper.ToDataModel).ToList();
    }

    public async Task<IReadOnlyList<HabitacionDataModel>> ListarPorSucursalAsync(Guid sucursalGuid, CancellationToken ct = default)
    {
        var s = await _uow.SucursalRepository.ObtenerPorGuidAsync(sucursalGuid, ct);
        if (s is null) return Array.Empty<HabitacionDataModel>();
        var list = await _uow.HabitacionRepository.ListarPorSucursalAsync(s.IdSucursal, ct);
        return list.Select(HabitacionDataMapper.ToDataModel).ToList();
    }

    public async Task<IReadOnlyList<HabitacionDataModel>> ListarDisponiblesAsync(Guid sucursalGuid, DateOnly inicio, DateOnly fin, CancellationToken ct = default)
    {
        var s = await _uow.SucursalRepository.ObtenerPorGuidAsync(sucursalGuid, ct);
        if (s is null) return Array.Empty<HabitacionDataModel>();
        var list = await _uow.HabitacionRepository.ListarDisponiblesAsync(s.IdSucursal, inicio, fin, ct);
        return list.Select(HabitacionDataMapper.ToDataModel).ToList();
    }

    public async Task<HabitacionDataModel> CrearAsync(HabitacionDataModel model, CancellationToken ct = default)
    {
        var entity = HabitacionDataMapper.ToEntity(model);
        if (entity.HabitacionGuid == Guid.Empty)
            entity.HabitacionGuid = Guid.NewGuid();
        await _uow.HabitacionRepository.AgregarAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        var reloaded = await _uow.HabitacionRepository.ObtenerPorIdAsync(entity.IdHabitacion, ct) ?? entity;
        return HabitacionDataMapper.ToDataModel(reloaded);
    }

    public async Task<HabitacionDataModel?> ActualizarAsync(HabitacionDataModel model, CancellationToken ct = default)
    {
        var exists = await _uow.HabitacionRepository.ObtenerPorIdAsync(model.IdHabitacion, ct);
        if (exists is null) return null;
        var incoming = HabitacionDataMapper.ToEntity(model);
        incoming.HabitacionGuid = exists.HabitacionGuid;
        incoming.FechaRegistroUtc = exists.FechaRegistroUtc;
        incoming.CreadoPorUsuario = exists.CreadoPorUsuario;
        incoming.IdSucursal = exists.IdSucursal;
        incoming.IdTipoHabitacion = exists.IdTipoHabitacion;
        _uow.HabitacionRepository.Actualizar(incoming);
        await _uow.SaveChangesAsync(ct);
        var reloaded = await _uow.HabitacionRepository.ObtenerPorIdAsync(model.IdHabitacion, ct);
        return reloaded is null ? null : HabitacionDataMapper.ToDataModel(reloaded);
    }

    public async Task CambiarEstadoAsync(Guid habitacionGuid, string nuevoEstado, string usuario, CancellationToken ct = default)
    {
        var entity = await _uow.HabitacionRepository.ObtenerParaActualizarPorGuidAsync(habitacionGuid, ct);
        if (entity is null) return;
        entity.EstadoHabitacion = nuevoEstado;
        entity.ModificadoPorUsuario = usuario;
        entity.FechaModificacionUtc = DateTimeOffset.UtcNow;
        _uow.HabitacionRepository.Actualizar(entity);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<bool> EliminarLogicoAsync(int idHabitacion, string usuario, CancellationToken ct = default)
    {
        var e = await _uow.HabitacionRepository.ObtenerParaActualizarAsync(idHabitacion, ct);
        if (e is null) return false;
        e.EsEliminado = true;
        e.ModificadoPorUsuario = usuario;
        e.FechaModificacionUtc = DateTimeOffset.UtcNow;
        _uow.HabitacionRepository.Actualizar(e);
        await _uow.SaveChangesAsync(ct);
        return true;
    }
}
