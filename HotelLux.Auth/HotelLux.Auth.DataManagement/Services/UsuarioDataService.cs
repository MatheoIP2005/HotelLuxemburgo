using HotelLux.Auth.DataManagement.Interfaces;
using HotelLux.Auth.DataManagement.Mappers;
using HotelLux.Auth.DataManagement.Models;

namespace HotelLux.Auth.DataManagement.Services;

public class UsuarioDataService : IUsuarioDataService
{
    private readonly IUnitOfWork _unitOfWork;

    public UsuarioDataService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<UsuarioDataModel?> ObtenerPorIdAsync(int idUsuario, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.UsuarioAppRepository.ObtenerPorIdAsync(idUsuario, cancellationToken);
        return entity is null ? null : UsuarioDataMapper.ToDataModel(entity);
    }

    public async Task<UsuarioDataModel?> ObtenerPorGuidAsync(Guid usuarioGuid, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.UsuarioAppRepository.ObtenerPorGuidAsync(usuarioGuid, cancellationToken);
        return entity is null ? null : UsuarioDataMapper.ToDataModel(entity);
    }

    public async Task<UsuarioDataModel?> ObtenerPorUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.UsuarioAppRepository.ObtenerPorUsernameAsync(username, cancellationToken);
        return entity is null ? null : UsuarioDataMapper.ToDataModel(entity);
    }

    public async Task<IReadOnlyList<UsuarioDataModel>> ListarAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _unitOfWork.UsuarioAppRepository.ListarAsync(cancellationToken);
        return entities.Select(UsuarioDataMapper.ToDataModel).ToList();
    }

    public async Task<LoginDataModel?> ObtenerParaLoginAsync(string username, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.UsuarioAppRepository.ObtenerPorUsernameAsync(username, cancellationToken);
        return entity is null ? null : UsuarioDataMapper.ToLoginDataModel(entity);
    }

    public async Task<UsuarioDataModel> CrearAsync(UsuarioDataModel model, CancellationToken cancellationToken = default)
    {
        var entity = UsuarioDataMapper.ToEntity(model);
        await _unitOfWork.UsuarioAppRepository.AgregarAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return UsuarioDataMapper.ToDataModel(entity);
    }

    public async Task<UsuarioDataModel?> ActualizarAsync(UsuarioDataModel model, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.UsuarioAppRepository.ObtenerParaActualizarAsync(model.IdUsuario, cancellationToken);
        if (entity is null) return null;

        entity.Username = model.Username;
        entity.Correo = model.Correo;
        entity.Nombres = model.Nombres;
        entity.Apellidos = model.Apellidos;
        entity.PasswordHash = model.PasswordHash;
        entity.PasswordSalt = model.PasswordSalt;
        entity.EstadoUsuario = model.EstadoUsuario;
        entity.Activo = model.Activo;
        entity.MotivoInhabilitacion = model.MotivoInhabilitacion;
        entity.FechaInhabilitacionUtc = model.FechaInhabilitacionUtc;
        entity.ModificadoPorUsuario = model.ModificadoPorUsuario;
        entity.FechaModificacionUtc = model.FechaModificacionUtc;
        entity.ModificacionIp = model.ModificacionIp;

        _unitOfWork.UsuarioAppRepository.Actualizar(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return UsuarioDataMapper.ToDataModel(entity);
    }

    public async Task<bool> EliminarLogicoAsync(int idUsuario, string usuario, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.UsuarioAppRepository.ObtenerParaActualizarAsync(idUsuario, cancellationToken);
        if (entity is null) return false;

        entity.EsEliminado = true;
        entity.Activo = false;
        entity.FechaInhabilitacionUtc = DateTimeOffset.UtcNow;
        entity.MotivoInhabilitacion = $"Eliminado por {usuario}";
        entity.ModificadoPorUsuario = usuario;

        _unitOfWork.UsuarioAppRepository.Actualizar(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
