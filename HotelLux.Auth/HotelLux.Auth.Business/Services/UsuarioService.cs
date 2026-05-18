using HotelLux.Auth.Business.DTOs.Roles;
using HotelLux.Auth.Business.DTOs.Usuarios;
using HotelLux.Auth.Business.Exceptions;
using HotelLux.Auth.Business.Interfaces;
using HotelLux.Auth.Business.Mappers;
using HotelLux.Auth.DataAccess.Entities;
using HotelLux.Auth.DataManagement.Interfaces;
using Microsoft.AspNetCore.Http;

namespace HotelLux.Auth.Business.Services;

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioDataService _usuarioDataService;
    private readonly IRolDataService _rolDataService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditEmitter _auditEmitter;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UsuarioService(
        IUsuarioDataService usuarioDataService,
        IRolDataService rolDataService,
        IUnitOfWork unitOfWork,
        IAuditEmitter auditEmitter,
        IHttpContextAccessor httpContextAccessor)
    {
        _usuarioDataService = usuarioDataService;
        _rolDataService = rolDataService;
        _unitOfWork = unitOfWork;
        _auditEmitter = auditEmitter;
        _httpContextAccessor = httpContextAccessor;
    }

    private string ActorUsuarioGuid()
        => _httpContextAccessor.HttpContext?.User?.FindFirst("usuario_guid")?.Value ?? string.Empty;

    public async Task<UsuarioDTO> ObtenerPorGuidAsync(Guid usuarioGuid, CancellationToken cancellationToken = default)
    {
        var model = await _usuarioDataService.ObtenerPorGuidAsync(usuarioGuid, cancellationToken);
        if (model is null)
            throw new NotFoundException("Usuario", usuarioGuid);
        return UsuarioBusinessMapper.ToDTO(model);
    }

    public async Task<IReadOnlyList<UsuarioDTO>> ListarAsync(CancellationToken cancellationToken = default)
    {
        var models = await _usuarioDataService.ListarAsync(cancellationToken);
        return models.Select(UsuarioBusinessMapper.ToDTO).ToList();
    }

    public async Task<IReadOnlyList<RolDTO>> ListarRolesAsync(Guid usuarioGuid, CancellationToken cancellationToken = default)
    {
        var model = await _usuarioDataService.ObtenerPorGuidAsync(usuarioGuid, cancellationToken);
        if (model is null)
            throw new NotFoundException("Usuario", usuarioGuid);

        var roles = await _rolDataService.ListarAsync(cancellationToken);
        return roles
            .Where(r => model.Roles.Contains(r.NombreRol))
            .Select(RolBusinessMapper.ToDTO)
            .ToList();
    }

    public async Task<UsuarioDTO> CrearAsync(UsuarioCreateDTO dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Username))
            throw new ValidationException("El nombre de usuario es obligatorio.");

        if (string.IsNullOrWhiteSpace(dto.Password))
            throw new ValidationException("La contraseña es obligatoria.");

        var existente = await _usuarioDataService.ObtenerPorUsernameAsync(dto.Username, cancellationToken);
        if (existente is not null)
            throw new ConflictException("Usuario", $"Ya existe un usuario con el nombre '{dto.Username}'.");

        var salt = string.Empty;
        var hash = BCrypt.Net.BCrypt.HashPassword(dto.Password, 11);

        var dataModel = UsuarioBusinessMapper.ToDataModel(dto, hash, salt);
        var creado = await _usuarioDataService.CrearAsync(dataModel, cancellationToken);
        var dtoResult = UsuarioBusinessMapper.ToDTO(creado);

        var actor = ActorUsuarioGuid();
        _ = _auditEmitter.EmitAsync(
            "usuario_app",
            "CREATE",
            creado.UsuarioGuid.ToString(),
            actor,
            "{}",
            CancellationToken.None);

        return dtoResult;
    }

    public async Task<UsuarioDTO> ActualizarAsync(Guid usuarioGuid, UsuarioUpdateDTO dto, CancellationToken cancellationToken = default)
    {
        var existente = await _usuarioDataService.ObtenerPorGuidAsync(usuarioGuid, cancellationToken);
        if (existente is null)
            throw new NotFoundException("Usuario", usuarioGuid);

        var dataModel = UsuarioBusinessMapper.ToDataModel(dto, existente);
        var actualizado = await _usuarioDataService.ActualizarAsync(dataModel, cancellationToken);
        var dtoResult = UsuarioBusinessMapper.ToDTO(actualizado!);

        var actor = ActorUsuarioGuid();
        _ = _auditEmitter.EmitAsync(
            "usuario_app",
            "UPDATE",
            usuarioGuid.ToString(),
            actor,
            "{}",
            CancellationToken.None);

        return dtoResult;
    }

    public async Task InhabilitarAsync(Guid usuarioGuid, string motivo, string usuario, CancellationToken cancellationToken = default)
    {
        var existente = await _usuarioDataService.ObtenerPorGuidAsync(usuarioGuid, cancellationToken);
        if (existente is null)
            throw new NotFoundException("Usuario", usuarioGuid);

        existente.EstadoUsuario = "BLO";
        existente.Activo = false;
        existente.MotivoInhabilitacion = motivo;
        existente.ModificadoPorUsuario = usuario;
        existente.FechaModificacionUtc = DateTimeOffset.UtcNow;
        await _usuarioDataService.ActualizarAsync(existente, cancellationToken);

        var actor = ActorUsuarioGuid();
        _ = _auditEmitter.EmitAsync(
            "usuario_app",
            "DISABLE",
            usuarioGuid.ToString(),
            actor,
            "{}",
            CancellationToken.None);
    }

    public async Task EliminarAsync(Guid usuarioGuid, string usuario, CancellationToken cancellationToken = default)
    {
        var existente = await _usuarioDataService.ObtenerPorGuidAsync(usuarioGuid, cancellationToken);
        if (existente is null)
            throw new NotFoundException("Usuario", usuarioGuid);

        await _usuarioDataService.EliminarLogicoAsync(existente.IdUsuario, usuario, cancellationToken);

        var actor = ActorUsuarioGuid();
        _ = _auditEmitter.EmitAsync(
            "usuario_app",
            "DELETE",
            usuarioGuid.ToString(),
            actor,
            "{}",
            CancellationToken.None);
    }

    public async Task AsignarRolAsync(Guid usuarioGuid, Guid rolGuid, string usuario, CancellationToken cancellationToken = default)
    {
        var existenteUsuario = await _usuarioDataService.ObtenerPorGuidAsync(usuarioGuid, cancellationToken);
        if (existenteUsuario is null)
            throw new NotFoundException("Usuario", usuarioGuid);

        var existenteRol = await _rolDataService.ObtenerPorGuidAsync(rolGuid, cancellationToken);
        if (existenteRol is null)
            throw new NotFoundException("Rol", rolGuid);

        var link = await _unitOfWork.UsuarioAppRepository.ObtenerUsuarioRolPorUsuarioYRolAsync(
            existenteUsuario.IdUsuario, existenteRol.IdRol, cancellationToken);

        if (link is null)
        {
            await _unitOfWork.UsuarioAppRepository.AgregarUsuarioRolAsync(new UsuarioRolEntity
            {
                IdUsuario = existenteUsuario.IdUsuario,
                IdRol = existenteRol.IdRol,
                EstadoUsuarioRol = "ACT",
                EsEliminado = false,
                Activo = true,
                CreadoPorUsuario = usuario,
                FechaRegistroUtc = DateTimeOffset.UtcNow
            }, cancellationToken);
        }
        else
        {
            if (link.Activo && link.EstadoUsuarioRol == "ACT" && !link.EsEliminado)
                throw new ConflictException("Usuario-Rol", $"El rol '{existenteRol.NombreRol}' ya está asignado al usuario.");

            link.EstadoUsuarioRol = "ACT";
            link.Activo = true;
            link.EsEliminado = false;
            link.ModificadoPorUsuario = usuario;
            link.FechaModificacionUtc = DateTimeOffset.UtcNow;
            _unitOfWork.UsuarioAppRepository.ActualizarUsuarioRol(link);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var actor = ActorUsuarioGuid();
        _ = _auditEmitter.EmitAsync(
            "usuario_app",
            "ASSIGN_ROLE",
            usuarioGuid.ToString(),
            actor,
            $"{{\"rolGuid\":\"{rolGuid}\"}}",
            CancellationToken.None);
    }

    public async Task RemoverRolAsync(Guid usuarioGuid, Guid rolGuid, string usuario, CancellationToken cancellationToken = default)
    {
        var existenteUsuario = await _usuarioDataService.ObtenerPorGuidAsync(usuarioGuid, cancellationToken);
        if (existenteUsuario is null)
            throw new NotFoundException("Usuario", usuarioGuid);

        var existenteRol = await _rolDataService.ObtenerPorGuidAsync(rolGuid, cancellationToken);
        if (existenteRol is null)
            throw new NotFoundException("Rol", rolGuid);

        var link = await _unitOfWork.UsuarioAppRepository.ObtenerUsuarioRolPorUsuarioYRolAsync(
            existenteUsuario.IdUsuario, existenteRol.IdRol, cancellationToken);
        if (link is null)
            throw new NotFoundException("Asignación usuario-rol", $"{usuarioGuid}/{rolGuid}");

        link.EstadoUsuarioRol = "INA";
        link.Activo = false;
        link.ModificadoPorUsuario = usuario;
        link.FechaModificacionUtc = DateTimeOffset.UtcNow;
        _unitOfWork.UsuarioAppRepository.ActualizarUsuarioRol(link);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var actor = ActorUsuarioGuid();
        _ = _auditEmitter.EmitAsync(
            "usuario_app",
            "REMOVE_ROLE",
            usuarioGuid.ToString(),
            actor,
            $"{{\"rolGuid\":\"{rolGuid}\"}}",
            CancellationToken.None);
    }
}
