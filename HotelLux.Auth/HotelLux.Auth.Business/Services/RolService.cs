using HotelLux.Auth.Business.DTOs.Roles;
using HotelLux.Auth.Business.Exceptions;
using HotelLux.Auth.Business.Interfaces;
using HotelLux.Auth.Business.Mappers;
using HotelLux.Auth.DataManagement.Interfaces;
using Microsoft.AspNetCore.Http;

namespace HotelLux.Auth.Business.Services;

public class RolService : IRolService
{
    private readonly IRolDataService _rolDataService;
    private readonly IAuditEmitter _auditEmitter;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RolService(IRolDataService rolDataService, IAuditEmitter auditEmitter, IHttpContextAccessor httpContextAccessor)
    {
        _rolDataService = rolDataService;
        _auditEmitter = auditEmitter;
        _httpContextAccessor = httpContextAccessor;
    }

    private string ActorUsuarioGuid()
        => _httpContextAccessor.HttpContext?.User?.FindFirst("usuario_guid")?.Value ?? string.Empty;

    public async Task<RolDTO> ObtenerPorGuidAsync(Guid rolGuid, CancellationToken cancellationToken = default)
    {
        var model = await _rolDataService.ObtenerPorGuidAsync(rolGuid, cancellationToken);
        if (model is null)
            throw new NotFoundException("Rol", rolGuid);
        return RolBusinessMapper.ToDTO(model);
    }

    public async Task<IReadOnlyList<RolDTO>> ListarAsync(CancellationToken cancellationToken = default)
    {
        var models = await _rolDataService.ListarAsync(cancellationToken);
        return models.Select(RolBusinessMapper.ToDTO).ToList();
    }

    public async Task<RolDTO> CrearAsync(RolCreateDTO dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.NombreRol))
            throw new ValidationException("El nombre del rol es obligatorio.");

        var existente = await _rolDataService.ObtenerPorNombreAsync(dto.NombreRol, cancellationToken);
        if (existente is not null)
            throw new ConflictException("Rol", $"Ya existe un rol con el nombre '{dto.NombreRol}'.");

        var dataModel = RolBusinessMapper.ToDataModel(dto);
        var creado = await _rolDataService.CrearAsync(dataModel, cancellationToken);
        var dtoResult = RolBusinessMapper.ToDTO(creado);

        var actor = ActorUsuarioGuid();
        _ = _auditEmitter.EmitAsync(
            "rol",
            "CREATE",
            creado.RolGuid.ToString(),
            actor,
            "{}",
            CancellationToken.None);

        return dtoResult;
    }

    public async Task<RolDTO> ActualizarAsync(Guid rolGuid, RolUpdateDTO dto, CancellationToken cancellationToken = default)
    {
        var existente = await _rolDataService.ObtenerPorGuidAsync(rolGuid, cancellationToken);
        if (existente is null)
            throw new NotFoundException("Rol", rolGuid);

        var dataModel = RolBusinessMapper.ToDataModel(dto, existente);
        var actualizado = await _rolDataService.ActualizarAsync(dataModel, cancellationToken);
        var dtoResult = RolBusinessMapper.ToDTO(actualizado!);

        var actor = ActorUsuarioGuid();
        _ = _auditEmitter.EmitAsync(
            "rol",
            "UPDATE",
            rolGuid.ToString(),
            actor,
            "{}",
            CancellationToken.None);

        return dtoResult;
    }

    public async Task InhabilitarAsync(Guid rolGuid, string usuario, CancellationToken cancellationToken = default)
    {
        var existente = await _rolDataService.ObtenerPorGuidAsync(rolGuid, cancellationToken);
        if (existente is null)
            throw new NotFoundException("Rol", rolGuid);

        existente.EstadoRol = "INA";
        existente.Activo = false;
        existente.FechaModificacionUtc = DateTimeOffset.UtcNow;
        existente.ModificadoPorUsuario = usuario;
        await _rolDataService.ActualizarAsync(existente, cancellationToken);

        var actor = ActorUsuarioGuid();
        _ = _auditEmitter.EmitAsync(
            "rol",
            "DISABLE",
            rolGuid.ToString(),
            actor,
            "{}",
            CancellationToken.None);
    }

    public async Task EliminarAsync(Guid rolGuid, string usuario, CancellationToken cancellationToken = default)
    {
        var existente = await _rolDataService.ObtenerPorGuidAsync(rolGuid, cancellationToken);
        if (existente is null)
            throw new NotFoundException("Rol", rolGuid);

        await _rolDataService.EliminarLogicoAsync(existente.IdRol, usuario, cancellationToken);

        var actor = ActorUsuarioGuid();
        _ = _auditEmitter.EmitAsync(
            "rol",
            "DELETE",
            rolGuid.ToString(),
            actor,
            "{}",
            CancellationToken.None);
    }
}
