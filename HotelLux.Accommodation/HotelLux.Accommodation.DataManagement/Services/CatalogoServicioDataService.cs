using HotelLux.Accommodation.DataManagement.Interfaces;
using HotelLux.Accommodation.DataManagement.Mappers;
using HotelLux.Accommodation.DataManagement.Models;

namespace HotelLux.Accommodation.DataManagement.Services;

public class CatalogoServicioDataService : ICatalogoServicioDataService
{
    private readonly IUnitOfWork _uow;

    public CatalogoServicioDataService(IUnitOfWork uow) => _uow = uow;

    public async Task<CatalogoServicioDataModel?> ObtenerPorIdAsync(int idCatalogo, CancellationToken ct = default)
    {
        var e = await _uow.CatalogoServicioRepository.ObtenerPorIdAsync(idCatalogo, ct);
        return e is null ? null : CatalogoServicioDataMapper.ToDataModel(e);
    }

    public async Task<CatalogoServicioDataModel?> ObtenerPorGuidAsync(Guid guid, CancellationToken ct = default)
    {
        var e = await _uow.CatalogoServicioRepository.ObtenerPorGuidAsync(guid, ct);
        return e is null ? null : CatalogoServicioDataMapper.ToDataModel(e);
    }

    public async Task<IReadOnlyList<CatalogoServicioDataModel>> ListarAsync(CancellationToken ct = default)
    {
        var list = await _uow.CatalogoServicioRepository.ListarAsync(ct);
        return list.Select(CatalogoServicioDataMapper.ToDataModel).ToList();
    }

    public async Task<IReadOnlyList<CatalogoServicioDataModel>> ListarPorSucursalAsync(int? idSucursal, CancellationToken ct = default)
    {
        var list = await _uow.CatalogoServicioRepository.ListarPorSucursalAsync(idSucursal, ct);
        return list.Select(CatalogoServicioDataMapper.ToDataModel).ToList();
    }

    public async Task<CatalogoServicioDataModel> CrearAsync(CatalogoServicioDataModel model, CancellationToken ct = default)
    {
        var entity = CatalogoServicioDataMapper.ToEntity(model);
        if (entity.CatalogoGuid == Guid.Empty)
            entity.CatalogoGuid = Guid.NewGuid();
        await _uow.CatalogoServicioRepository.AgregarAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        var reloaded = await _uow.CatalogoServicioRepository.ObtenerPorIdAsync(entity.IdCatalogo, ct) ?? entity;
        return CatalogoServicioDataMapper.ToDataModel(reloaded);
    }

    public async Task<CatalogoServicioDataModel?> ActualizarAsync(CatalogoServicioDataModel model, CancellationToken ct = default)
    {
        var exists = await _uow.CatalogoServicioRepository.ObtenerPorIdAsync(model.IdCatalogo, ct);
        if (exists is null) return null;
        var incoming = CatalogoServicioDataMapper.ToEntity(model);
        incoming.CatalogoGuid = exists.CatalogoGuid;
        incoming.FechaRegistroUtc = exists.FechaRegistroUtc;
        incoming.CreadoPorUsuario = exists.CreadoPorUsuario;
        _uow.CatalogoServicioRepository.Actualizar(incoming);
        await _uow.SaveChangesAsync(ct);
        var reloaded = await _uow.CatalogoServicioRepository.ObtenerPorIdAsync(model.IdCatalogo, ct);
        return reloaded is null ? null : CatalogoServicioDataMapper.ToDataModel(reloaded);
    }

    public async Task DesactivarAsync(Guid catalogoGuid, string usuario, CancellationToken ct = default)
    {
        var e = await _uow.CatalogoServicioRepository.ObtenerParaActualizarPorGuidAsync(catalogoGuid, ct);
        if (e is null) return;
        e.EstadoCatalogo = "INA";
        e.FechaInhabilitacionUtc = DateTimeOffset.UtcNow;
        e.MotivoInhabilitacion = $"Desactivado por {usuario}";
        e.ModificadoPorUsuario = usuario;
        e.FechaModificacionUtc = DateTimeOffset.UtcNow;
        _uow.CatalogoServicioRepository.Actualizar(e);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<bool> EliminarLogicoAsync(int idCatalogo, string usuario, CancellationToken ct = default)
    {
        var e = await _uow.CatalogoServicioRepository.ObtenerParaActualizarAsync(idCatalogo, ct);
        if (e is null) return false;
        e.EsEliminado = true;
        e.ModificadoPorUsuario = usuario;
        e.FechaModificacionUtc = DateTimeOffset.UtcNow;
        _uow.CatalogoServicioRepository.Actualizar(e);
        await _uow.SaveChangesAsync(ct);
        return true;
    }
}
