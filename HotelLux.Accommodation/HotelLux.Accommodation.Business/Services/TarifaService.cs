using System.Text.Json;
using HotelLux.Accommodation.Business.DTOs.Tarifa;
using HotelLux.Accommodation.Business.Exceptions;
using HotelLux.Accommodation.Business.Interfaces;
using HotelLux.Accommodation.Business.Mappers;
using HotelLux.Accommodation.Business.Validators;
using HotelLux.Accommodation.DataManagement.Interfaces;

namespace HotelLux.Accommodation.Business.Services;

public class TarifaService : ITarifaService
{
    private readonly ITarifaDataService _dataService;
    private readonly ISucursalDataService _sucursalDataService;
    private readonly ITipoHabitacionDataService _tipoDataService;
    private readonly IAuditEmitter _audit;

    public TarifaService(
        ITarifaDataService dataService,
        ISucursalDataService sucursalDataService,
        ITipoHabitacionDataService tipoDataService,
        IAuditEmitter audit)
    {
        _dataService = dataService;
        _sucursalDataService = sucursalDataService;
        _tipoDataService = tipoDataService;
        _audit = audit;
    }

    public async Task<TarifaDTO> ObtenerPorGuidAsync(Guid guid, CancellationToken ct = default)
    {
        var model = await _dataService.ObtenerPorGuidAsync(guid, ct);
        if (model is null) throw new NotFoundException("Tarifa", guid);
        return TarifaBusinessMapper.ToDTO(model);
    }

    public async Task<IReadOnlyList<TarifaDTO>> ListarAsync(CancellationToken ct = default)
    {
        var models = await _dataService.ListarAsync(ct);
        return models.Select(TarifaBusinessMapper.ToDTO).ToList();
    }

    public async Task<TarifaDTO> CrearAsync(TarifaCreateDTO dto, CancellationToken ct = default)
    {
        var errors = TarifaValidator.ValidarCreacion(dto);
        if (errors.Count != 0) throw new ValidationException("Solicitud de creación inválida.", errors);

        var sucursal = await _sucursalDataService.ObtenerPorGuidAsync(dto.SucursalGuid, ct);
        if (sucursal is null) throw new NotFoundException("Sucursal", dto.SucursalGuid);

        var tipo = await _tipoDataService.ObtenerPorGuidAsync(dto.TipoHabitacionGuid, ct);
        if (tipo is null) throw new NotFoundException("TipoHabitación", dto.TipoHabitacionGuid);

        var dataModel = TarifaBusinessMapper.ToDataModel(dto, sucursal.IdSucursal, tipo.IdTipoHabitacion);
        var creado = await _dataService.CrearAsync(dataModel, ct);

        _audit.EmitFireAndForget("accommodation-service", "alojamiento.tarifa", "INSERT",
            creado.TarifaGuid.ToString(), creado.IdTarifa.ToString(),
            string.Empty, dto.CreadoPorUsuario ?? "api_user", dto.CreadoDesdeIp,
            null, JsonSerializer.Serialize(creado));

        return TarifaBusinessMapper.ToDTO(creado);
    }

    public async Task<TarifaDTO> ActualizarAsync(Guid guid, TarifaUpdateDTO dto, CancellationToken ct = default)
    {
        var errors = TarifaValidator.ValidarActualizacion(dto);
        if (errors.Count != 0) throw new ValidationException("Solicitud de actualización inválida.", errors);

        var existente = await _dataService.ObtenerPorGuidAsync(guid, ct);
        if (existente is null) throw new NotFoundException("Tarifa", guid);

        var anterior = JsonSerializer.Serialize(existente);
        var dataModel = TarifaBusinessMapper.ToDataModel(dto, existente);
        var actualizado = await _dataService.ActualizarAsync(dataModel, ct);

        _audit.EmitFireAndForget("accommodation-service", "alojamiento.tarifa", "UPDATE",
            guid.ToString(), existente.IdTarifa.ToString(),
            string.Empty, dto.ModificadoPorUsuario ?? "api_user", dto.ModificadoDesdeIp,
            anterior, JsonSerializer.Serialize(actualizado));

        return TarifaBusinessMapper.ToDTO(actualizado!);
    }

    public async Task DesactivarAsync(Guid guid, string usuario, CancellationToken ct = default)
    {
        var existente = await _dataService.ObtenerPorGuidAsync(guid, ct);
        if (existente is null) throw new NotFoundException("Tarifa", guid);

        await _dataService.DesactivarAsync(guid, usuario, ct);

        _audit.EmitFireAndForget("accommodation-service", "alojamiento.tarifa", "UPDATE",
            guid.ToString(), existente.IdTarifa.ToString(),
            string.Empty, usuario, null, JsonSerializer.Serialize(existente), null);
    }

    public async Task EliminarAsync(Guid guid, string usuario, CancellationToken ct = default)
    {
        var existente = await _dataService.ObtenerPorGuidAsync(guid, ct);
        if (existente is null) throw new NotFoundException("Tarifa", guid);

        await _dataService.EliminarLogicoAsync(existente.IdTarifa, usuario, ct);

        _audit.EmitFireAndForget("accommodation-service", "alojamiento.tarifa", "DELETE",
            guid.ToString(), existente.IdTarifa.ToString(),
            string.Empty, usuario, null,
            JsonSerializer.Serialize(existente), null);
    }
}
