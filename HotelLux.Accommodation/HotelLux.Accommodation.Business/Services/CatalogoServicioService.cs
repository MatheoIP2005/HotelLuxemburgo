using System.Text.Json;
using HotelLux.Accommodation.Business.DTOs.CatalogoServicio;
using HotelLux.Accommodation.Business.Exceptions;
using HotelLux.Accommodation.Business.Interfaces;
using HotelLux.Accommodation.Business.Mappers;
using HotelLux.Accommodation.Business.Validators;
using HotelLux.Accommodation.DataManagement.Interfaces;

namespace HotelLux.Accommodation.Business.Services;

public class CatalogoServicioService : ICatalogoServicioService
{
    private readonly ICatalogoServicioDataService _dataService;
    private readonly ISucursalDataService _sucursalDataService;
    private readonly IAuditEmitter _audit;

    public CatalogoServicioService(
        ICatalogoServicioDataService dataService,
        ISucursalDataService sucursalDataService,
        IAuditEmitter audit)
    {
        _dataService = dataService;
        _sucursalDataService = sucursalDataService;
        _audit = audit;
    }

    public async Task<CatalogoServicioDTO> ObtenerPorGuidAsync(Guid guid, CancellationToken ct = default)
    {
        var model = await _dataService.ObtenerPorGuidAsync(guid, ct);
        if (model is null) throw new NotFoundException("CatálogoServicio", guid);
        return CatalogoServicioBusinessMapper.ToDTO(model);
    }

    public async Task<IReadOnlyList<CatalogoServicioDTO>> ListarAsync(CancellationToken ct = default)
    {
        var models = await _dataService.ListarAsync(ct);
        return models.Select(CatalogoServicioBusinessMapper.ToDTO).ToList();
    }

    public async Task<CatalogoServicioDTO> CrearAsync(CatalogoServicioCreateDTO dto, CancellationToken ct = default)
    {
        var errors = CatalogoServicioValidator.ValidarCreacion(dto);
        if (errors.Count != 0) throw new ValidationException("Solicitud de creación inválida.", errors);

        int? idSucursal = null;
        if (dto.SucursalGuid.HasValue)
        {
            var s = await _sucursalDataService.ObtenerPorGuidAsync(dto.SucursalGuid.Value, ct);
            if (s is null) throw new NotFoundException("Sucursal", dto.SucursalGuid.Value);
            idSucursal = s.IdSucursal;
        }

        var dataModel = CatalogoServicioBusinessMapper.ToDataModel(dto, idSucursal);
        var creado = await _dataService.CrearAsync(dataModel, ct);

        _audit.EmitFireAndForget("accommodation-service", "alojamiento.catalogo_servicios", "INSERT",
            creado.CatalogoGuid.ToString(), creado.IdCatalogo.ToString(),
            string.Empty, dto.CreadoPorUsuario ?? "api_user", dto.CreadoDesdeIp,
            null, JsonSerializer.Serialize(creado));

        return CatalogoServicioBusinessMapper.ToDTO(creado);
    }

    public async Task<CatalogoServicioDTO> ActualizarAsync(Guid guid, CatalogoServicioUpdateDTO dto, CancellationToken ct = default)
    {
        var errors = CatalogoServicioValidator.ValidarActualizacion(dto);
        if (errors.Count != 0) throw new ValidationException("Solicitud de actualización inválida.", errors);

        var existente = await _dataService.ObtenerPorGuidAsync(guid, ct);
        if (existente is null) throw new NotFoundException("CatálogoServicio", guid);

        int? idSucursal = null;
        if (dto.SucursalGuid.HasValue)
        {
            var s = await _sucursalDataService.ObtenerPorGuidAsync(dto.SucursalGuid.Value, ct);
            if (s is null) throw new NotFoundException("Sucursal", dto.SucursalGuid.Value);
            idSucursal = s.IdSucursal;
        }

        var anterior = JsonSerializer.Serialize(existente);
        var dataModel = CatalogoServicioBusinessMapper.ToDataModel(dto, existente, idSucursal);
        var actualizado = await _dataService.ActualizarAsync(dataModel, ct);

        _audit.EmitFireAndForget("accommodation-service", "alojamiento.catalogo_servicios", "UPDATE",
            guid.ToString(), existente.IdCatalogo.ToString(),
            string.Empty, dto.ModificadoPorUsuario ?? "api_user", dto.ModificadoDesdeIp,
            anterior, JsonSerializer.Serialize(actualizado));

        return CatalogoServicioBusinessMapper.ToDTO(actualizado!);
    }

    public async Task DesactivarAsync(Guid guid, string usuario, CancellationToken ct = default)
    {
        var existente = await _dataService.ObtenerPorGuidAsync(guid, ct);
        if (existente is null) throw new NotFoundException("CatálogoServicio", guid);

        await _dataService.DesactivarAsync(guid, usuario, ct);

        _audit.EmitFireAndForget("accommodation-service", "alojamiento.catalogo_servicios", "UPDATE",
            guid.ToString(), existente.IdCatalogo.ToString(),
            string.Empty, usuario, null, JsonSerializer.Serialize(existente), null);
    }

    public async Task EliminarAsync(Guid guid, string usuario, CancellationToken ct = default)
    {
        var existente = await _dataService.ObtenerPorGuidAsync(guid, ct);
        if (existente is null) throw new NotFoundException("CatálogoServicio", guid);

        await _dataService.EliminarLogicoAsync(existente.IdCatalogo, usuario, ct);

        _audit.EmitFireAndForget("accommodation-service", "alojamiento.catalogo_servicios", "DELETE",
            guid.ToString(), existente.IdCatalogo.ToString(),
            string.Empty, usuario, null,
            JsonSerializer.Serialize(existente), null);
    }
}
