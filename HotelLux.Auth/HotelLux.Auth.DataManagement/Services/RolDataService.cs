using HotelLux.Auth.DataManagement.Interfaces;
using HotelLux.Auth.DataManagement.Mappers;
using HotelLux.Auth.DataManagement.Models;

namespace HotelLux.Auth.DataManagement.Services;

public class RolDataService : IRolDataService
{
    private readonly IUnitOfWork _unitOfWork;

    public RolDataService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<RolDataModel?> ObtenerPorIdAsync(int idRol, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.RolRepository.ObtenerPorIdAsync(idRol, cancellationToken);
        return entity is null ? null : RolDataMapper.ToDataModel(entity);
    }

    public async Task<RolDataModel?> ObtenerPorGuidAsync(Guid rolGuid, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.RolRepository.ObtenerPorGuidAsync(rolGuid, cancellationToken);
        return entity is null ? null : RolDataMapper.ToDataModel(entity);
    }

    public async Task<RolDataModel?> ObtenerPorNombreAsync(string nombreRol, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.RolRepository.ObtenerPorNombreAsync(nombreRol, cancellationToken);
        return entity is null ? null : RolDataMapper.ToDataModel(entity);
    }

    public async Task<IReadOnlyList<RolDataModel>> ListarAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _unitOfWork.RolRepository.ListarAsync(cancellationToken);
        return entities.Select(RolDataMapper.ToDataModel).ToList();
    }

    public async Task<RolDataModel> CrearAsync(RolDataModel model, CancellationToken cancellationToken = default)
    {
        var entity = RolDataMapper.ToEntity(model);
        await _unitOfWork.RolRepository.AgregarAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return RolDataMapper.ToDataModel(entity);
    }

    public async Task<RolDataModel?> ActualizarAsync(RolDataModel model, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.RolRepository.ObtenerParaActualizarAsync(model.IdRol, cancellationToken);
        if (entity is null) return null;

        entity.NombreRol = model.NombreRol;
        entity.DescripcionRol = model.DescripcionRol;
        entity.EstadoRol = model.EstadoRol;
        entity.Activo = model.Activo;
        entity.ModificadoPorUsuario = model.ModificadoPorUsuario;
        entity.FechaModificacionUtc = model.FechaModificacionUtc;
        entity.ModificacionIp = model.ModificacionIp;

        _unitOfWork.RolRepository.Actualizar(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return RolDataMapper.ToDataModel(entity);
    }

    public async Task<bool> EliminarLogicoAsync(int idRol, string usuario, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.RolRepository.ObtenerParaActualizarAsync(idRol, cancellationToken);
        if (entity is null) return false;

        entity.EsEliminado = true;
        entity.Activo = false;
        entity.FechaInhabilitacionUtc = DateTimeOffset.UtcNow;
        entity.MotivoInhabilitacion = $"Eliminado por {usuario}";
        entity.ModificadoPorUsuario = usuario;

        _unitOfWork.RolRepository.Actualizar(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
