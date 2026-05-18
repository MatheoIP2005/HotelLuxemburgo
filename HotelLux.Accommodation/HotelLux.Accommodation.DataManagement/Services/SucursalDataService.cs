using HotelLux.Accommodation.DataAccess.Entities;
using HotelLux.Accommodation.DataManagement.Interfaces;
using HotelLux.Accommodation.DataManagement.Mappers;
using HotelLux.Accommodation.DataManagement.Models;

namespace HotelLux.Accommodation.DataManagement.Services;

public class SucursalDataService : ISucursalDataService
{
    private readonly IUnitOfWork _uow;

    public SucursalDataService(IUnitOfWork uow) => _uow = uow;

    public async Task<SucursalDataModel?> ObtenerPorIdAsync(int idSucursal, CancellationToken ct = default)
    {
        var e = await _uow.SucursalRepository.ObtenerPorIdAsync(idSucursal, ct);
        return e is null ? null : SucursalDataMapper.ToDataModel(e);
    }

    public async Task<SucursalDataModel?> ObtenerPorGuidAsync(Guid guid, CancellationToken ct = default)
    {
        var e = await _uow.SucursalRepository.ObtenerPorGuidAsync(guid, ct);
        return e is null ? null : SucursalDataMapper.ToDataModel(e);
    }

    public async Task<IReadOnlyList<SucursalDataModel>> ListarAsync(CancellationToken ct = default)
    {
        var list = await _uow.SucursalRepository.ListarAsync(ct);
        return list.Select(SucursalDataMapper.ToDataModel).ToList();
    }

    public async Task<SucursalDataModel> CrearAsync(SucursalDataModel model, CancellationToken ct = default)
    {
        var entity = SucursalDataMapper.ToEntity(model);
        if (entity.SucursalGuid == Guid.Empty)
            entity.SucursalGuid = Guid.NewGuid();
        await _uow.SucursalRepository.AgregarAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        var reloaded = await _uow.SucursalRepository.ObtenerPorIdAsync(entity.IdSucursal, ct)
            ?? entity;
        return SucursalDataMapper.ToDataModel(reloaded);
    }

    public async Task<SucursalDataModel?> ActualizarAsync(SucursalDataModel model, CancellationToken ct = default)
    {
        var exists = await _uow.SucursalRepository.ObtenerPorIdAsync(model.IdSucursal, ct);
        if (exists is null) return null;
        var incoming = SucursalDataMapper.ToEntity(model);
        incoming.SucursalGuid = exists.SucursalGuid;
        incoming.FechaRegistroUtc = exists.FechaRegistroUtc;
        incoming.CreadoPorUsuario = exists.CreadoPorUsuario;
        _uow.SucursalRepository.Actualizar(incoming);
        await _uow.SaveChangesAsync(ct);
        var reloaded = await _uow.SucursalRepository.ObtenerPorIdAsync(model.IdSucursal, ct);
        return reloaded is null ? null : SucursalDataMapper.ToDataModel(reloaded);
    }

    public async Task<bool> EliminarLogicoAsync(int idSucursal, string usuario, CancellationToken ct = default)
    {
        var e = await _uow.SucursalRepository.ObtenerParaActualizarAsync(idSucursal, ct);
        if (e is null) return false;
        e.EsEliminado = true;
        e.ModificadoPorUsuario = usuario;
        e.FechaModificacionUtc = DateTimeOffset.UtcNow;
        _uow.SucursalRepository.Actualizar(e);
        await _uow.SaveChangesAsync(ct);
        return true;
    }
}
