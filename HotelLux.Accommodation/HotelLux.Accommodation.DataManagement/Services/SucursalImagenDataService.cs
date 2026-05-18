using HotelLux.Accommodation.DataManagement.Interfaces;
using HotelLux.Accommodation.DataManagement.Mappers;
using HotelLux.Accommodation.DataManagement.Models;

namespace HotelLux.Accommodation.DataManagement.Services;

public class SucursalImagenDataService : ISucursalImagenDataService
{
    private readonly IUnitOfWork _uow;

    public SucursalImagenDataService(IUnitOfWork uow) => _uow = uow;

    public async Task<IReadOnlyList<SucursalImagenDataModel>> ListarPorSucursalAsync(int idSucursal, CancellationToken ct = default)
    {
        var list = await _uow.SucursalImagenRepository.ListarPorSucursalAsync(idSucursal, ct);
        return list.Select(SucursalImagenDataMapper.ToDataModel).ToList();
    }

    public async Task<IReadOnlyList<SucursalImagenDataModel>> ListarPorSucursalGuidAsync(Guid sucursalGuid, CancellationToken ct = default)
    {
        var s = await _uow.SucursalRepository.ObtenerPorGuidAsync(sucursalGuid, ct);
        if (s is null) return Array.Empty<SucursalImagenDataModel>();
        return await ListarPorSucursalAsync(s.IdSucursal, ct);
    }

    public async Task<SucursalImagenDataModel> CrearAsync(SucursalImagenDataModel model, CancellationToken ct = default)
    {
        var entity = SucursalImagenDataMapper.ToEntity(model);
        if (entity.SucursalImagenGuid == Guid.Empty)
            entity.SucursalImagenGuid = Guid.NewGuid();
        await _uow.SucursalImagenRepository.AgregarAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        var reloaded = await _uow.SucursalImagenRepository.ObtenerPorGuidAsync(entity.SucursalImagenGuid, ct)
            ?? entity;
        return SucursalImagenDataMapper.ToDataModel(reloaded);
    }

    public async Task EliminarAsync(Guid imagenGuid, CancellationToken ct = default)
    {
        var e = await _uow.SucursalImagenRepository.ObtenerPorGuidAsync(imagenGuid, ct);
        if (e is null) return;
        _uow.SucursalImagenRepository.Eliminar(e);
        await _uow.SaveChangesAsync(ct);
    }
}
