using System.Text.Json;
using HotelLux.Accommodation.Business.DTOs.TipoHabitacion;
using HotelLux.Accommodation.Business.Exceptions;
using HotelLux.Accommodation.Business.Interfaces;
using HotelLux.Accommodation.Business.Mappers;
using HotelLux.Accommodation.Business.Validators;
using HotelLux.Accommodation.DataManagement.Interfaces;

namespace HotelLux.Accommodation.Business.Services;

public class TipoHabitacionService : ITipoHabitacionService
{
    private readonly ITipoHabitacionDataService _dataService;
    private readonly IAuditEmitter _audit;

    public TipoHabitacionService(ITipoHabitacionDataService dataService, IAuditEmitter audit)
    {
        _dataService = dataService;
        _audit = audit;
    }

    public async Task<TipoHabitacionDTO> ObtenerPorGuidAsync(Guid guid, CancellationToken ct = default)
    {
        var model = await _dataService.ObtenerPorGuidAsync(guid, ct);
        if (model is null) throw new NotFoundException("TipoHabitación", guid);
        return TipoHabitacionBusinessMapper.ToDTO(model);
    }

    public async Task<IReadOnlyList<TipoHabitacionDTO>> ListarAsync(CancellationToken ct = default)
    {
        var models = await _dataService.ListarAsync(ct);
        return models.Select(TipoHabitacionBusinessMapper.ToDTO).ToList();
    }

    public async Task<TipoHabitacionDTO> CrearAsync(TipoHabitacionCreateDTO dto, CancellationToken ct = default)
    {
        var errors = TipoHabitacionValidator.ValidarCreacion(dto);
        if (errors.Count != 0) throw new ValidationException("Solicitud de creación inválida.", errors);

        var dataModel = TipoHabitacionBusinessMapper.ToDataModel(dto);
        var creado = await _dataService.CrearAsync(dataModel, ct);

        _audit.EmitFireAndForget("accommodation-service", "alojamiento.tipo_habitacion", "INSERT",
            creado.TipoHabitacionGuid.ToString(), creado.IdTipoHabitacion.ToString(),
            string.Empty, dto.CreadoPorUsuario ?? "api_user", dto.CreadoDesdeIp,
            null, JsonSerializer.Serialize(creado));

        return TipoHabitacionBusinessMapper.ToDTO(creado);
    }

    public async Task<TipoHabitacionDTO> ActualizarAsync(Guid guid, TipoHabitacionUpdateDTO dto, CancellationToken ct = default)
    {
        var errors = TipoHabitacionValidator.ValidarActualizacion(dto);
        if (errors.Count != 0) throw new ValidationException("Solicitud de actualización inválida.", errors);

        var existente = await _dataService.ObtenerPorGuidAsync(guid, ct);
        if (existente is null) throw new NotFoundException("TipoHabitación", guid);

        var anterior = JsonSerializer.Serialize(existente);
        var dataModel = TipoHabitacionBusinessMapper.ToDataModel(dto, existente);
        var actualizado = await _dataService.ActualizarAsync(dataModel, ct);

        _audit.EmitFireAndForget("accommodation-service", "alojamiento.tipo_habitacion", "UPDATE",
            guid.ToString(), existente.IdTipoHabitacion.ToString(),
            string.Empty, dto.ModificadoPorUsuario ?? "api_user", dto.ModificadoDesdeIp,
            anterior, JsonSerializer.Serialize(actualizado));

        return TipoHabitacionBusinessMapper.ToDTO(actualizado!);
    }

    public async Task InhabilitarAsync(Guid guid, string usuario, CancellationToken ct = default)
    {
        var existente = await _dataService.ObtenerPorGuidAsync(guid, ct);
        if (existente is null) throw new NotFoundException("TipoHabitación", guid);

        var anterior = JsonSerializer.Serialize(existente);
        existente.EstadoTipoHabitacion = "INA";
        existente.FechaInhabilitacionUtc = DateTimeOffset.UtcNow;
        existente.MotivoInhabilitacion = $"Inhabilitado por {usuario}";
        existente.ModificadoPorUsuario = usuario;
        existente.FechaModificacionUtc = DateTimeOffset.UtcNow;
        await _dataService.ActualizarAsync(existente, ct);

        _audit.EmitFireAndForget("accommodation-service", "alojamiento.tipo_habitacion", "UPDATE",
            guid.ToString(), existente.IdTipoHabitacion.ToString(),
            string.Empty, usuario, null, anterior, null);
    }

    public async Task EliminarAsync(Guid guid, string usuario, CancellationToken ct = default)
    {
        var existente = await _dataService.ObtenerPorGuidAsync(guid, ct);
        if (existente is null) throw new NotFoundException("TipoHabitación", guid);

        await _dataService.EliminarLogicoAsync(existente.IdTipoHabitacion, usuario, ct);

        _audit.EmitFireAndForget("accommodation-service", "alojamiento.tipo_habitacion", "DELETE",
            guid.ToString(), existente.IdTipoHabitacion.ToString(),
            string.Empty, usuario, null,
            JsonSerializer.Serialize(existente), null);
    }
}
