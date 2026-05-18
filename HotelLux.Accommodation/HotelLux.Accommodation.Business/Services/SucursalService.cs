using System.Text.Json;
using HotelLux.Accommodation.Business.DTOs.Sucursal;
using HotelLux.Accommodation.Business.Exceptions;
using HotelLux.Accommodation.Business.Interfaces;
using HotelLux.Accommodation.Business.Mappers;
using HotelLux.Accommodation.Business.Validators;
using HotelLux.Accommodation.DataManagement.Interfaces;

namespace HotelLux.Accommodation.Business.Services;

public class SucursalService : ISucursalService
{
    private readonly ISucursalDataService _dataService;
    private readonly IAuditEmitter _audit;

    public SucursalService(ISucursalDataService dataService, IAuditEmitter audit)
    {
        _dataService = dataService;
        _audit = audit;
    }

    public async Task<SucursalDTO> ObtenerPorGuidAsync(Guid guid, CancellationToken ct = default)
    {
        var model = await _dataService.ObtenerPorGuidAsync(guid, ct);
        if (model is null) throw new NotFoundException("Sucursal", guid);
        return SucursalBusinessMapper.ToDTO(model);
    }

    public async Task<IReadOnlyList<SucursalDTO>> ListarAsync(CancellationToken ct = default)
    {
        var models = await _dataService.ListarAsync(ct);
        return models.Select(SucursalBusinessMapper.ToDTO).ToList();
    }

    public async Task<SucursalDTO> CrearAsync(SucursalCreateDTO dto, CancellationToken ct = default)
    {
        var errors = SucursalValidator.ValidarCreacion(dto);
        if (errors.Count != 0) throw new ValidationException("Solicitud de creación inválida.", errors);

        var dataModel = SucursalBusinessMapper.ToDataModel(dto);
        var creado = await _dataService.CrearAsync(dataModel, ct);

        _audit.EmitFireAndForget("accommodation-service", "alojamiento.sucursal", "INSERT",
            creado.SucursalGuid.ToString(), creado.IdSucursal.ToString(),
            string.Empty, dto.CreadoPorUsuario ?? "api_user", dto.CreadoDesdeIp,
            null, JsonSerializer.Serialize(creado));

        return SucursalBusinessMapper.ToDTO(creado);
    }

    public async Task<SucursalDTO> ActualizarAsync(Guid guid, SucursalUpdateDTO dto, CancellationToken ct = default)
    {
        var errors = SucursalValidator.ValidarActualizacion(dto);
        if (errors.Count != 0) throw new ValidationException("Solicitud de actualización inválida.", errors);

        var existente = await _dataService.ObtenerPorGuidAsync(guid, ct);
        if (existente is null) throw new NotFoundException("Sucursal", guid);

        var anterior = JsonSerializer.Serialize(existente);
        var dataModel = SucursalBusinessMapper.ToDataModel(dto, existente);
        var actualizado = await _dataService.ActualizarAsync(dataModel, ct);

        _audit.EmitFireAndForget("accommodation-service", "alojamiento.sucursal", "UPDATE",
            guid.ToString(), existente.IdSucursal.ToString(),
            string.Empty, dto.ModificadoPorUsuario ?? "api_user", dto.ModificadoDesdeIp,
            anterior, JsonSerializer.Serialize(actualizado));

        return SucursalBusinessMapper.ToDTO(actualizado!);
    }

    public async Task<SucursalDTO> ActualizarPoliticasAsync(Guid guid, SucursalPoliticasPatchDTO dto, CancellationToken ct = default)
    {
        var existente = await _dataService.ObtenerPorGuidAsync(guid, ct);
        if (existente is null) throw new NotFoundException("Sucursal", guid);

        var anterior = JsonSerializer.Serialize(existente);

        if (dto.HoraCheckin is not null) existente.HoraCheckin = dto.HoraCheckin;
        if (dto.HoraCheckout is not null) existente.HoraCheckout = dto.HoraCheckout;
        if (dto.AceptaNinos.HasValue) existente.AceptaNinos = dto.AceptaNinos.Value;
        if (dto.EdadMinimaHuesped.HasValue) existente.EdadMinimaHuesped = dto.EdadMinimaHuesped;
        if (dto.PermiteMascotas.HasValue) existente.PermiteMascotas = dto.PermiteMascotas.Value;
        if (dto.SePermiteFumar.HasValue) existente.SePermiteFumar = dto.SePermiteFumar.Value;
        if (dto.CheckinAnticipado.HasValue) existente.CheckinAnticipado = dto.CheckinAnticipado.Value;
        if (dto.CheckoutTardio.HasValue) existente.CheckoutTardio = dto.CheckoutTardio.Value;
        if (!string.IsNullOrWhiteSpace(dto.Politicas))
            existente.DescripcionCorta = dto.Politicas.Trim();

        existente.ModificadoPorUsuario = dto.ModificadoPorUsuario ?? "api_user";
        existente.ModificacionIp = dto.ModificadoDesdeIp;
        existente.FechaModificacionUtc = DateTimeOffset.UtcNow;

        var actualizado = await _dataService.ActualizarAsync(existente, ct);

        _audit.EmitFireAndForget("accommodation-service", "alojamiento.sucursal", "UPDATE",
            guid.ToString(), existente.IdSucursal.ToString(),
            string.Empty, existente.ModificadoPorUsuario, dto.ModificadoDesdeIp,
            anterior, JsonSerializer.Serialize(actualizado));

        return SucursalBusinessMapper.ToDTO(actualizado!);
    }

    public async Task InhabilitarAsync(Guid guid, string usuario, CancellationToken ct = default)
    {
        var existente = await _dataService.ObtenerPorGuidAsync(guid, ct);
        if (existente is null) throw new NotFoundException("Sucursal", guid);

        var anterior = JsonSerializer.Serialize(existente);
        existente.EstadoSucursal = "INA";
        existente.FechaInhabilitacionUtc = DateTimeOffset.UtcNow;
        existente.MotivoInhabilitacion = $"Inhabilitada por {usuario}";
        existente.ModificadoPorUsuario = usuario;
        existente.FechaModificacionUtc = DateTimeOffset.UtcNow;
        await _dataService.ActualizarAsync(existente, ct);

        _audit.EmitFireAndForget("accommodation-service", "alojamiento.sucursal", "UPDATE",
            guid.ToString(), existente.IdSucursal.ToString(),
            string.Empty, usuario, null, anterior, null);
    }

    public async Task EliminarAsync(Guid guid, string usuario, CancellationToken ct = default)
    {
        var existente = await _dataService.ObtenerPorGuidAsync(guid, ct);
        if (existente is null) throw new NotFoundException("Sucursal", guid);

        await _dataService.EliminarLogicoAsync(existente.IdSucursal, usuario, ct);

        _audit.EmitFireAndForget("accommodation-service", "alojamiento.sucursal", "DELETE",
            guid.ToString(), existente.IdSucursal.ToString(),
            string.Empty, usuario, null,
            JsonSerializer.Serialize(existente), null);
    }
}
