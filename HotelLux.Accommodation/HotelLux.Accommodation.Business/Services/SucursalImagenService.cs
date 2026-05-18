using System.Text.Json;
using HotelLux.Accommodation.Business.DTOs.SucursalImagen;
using HotelLux.Accommodation.Business.Exceptions;
using HotelLux.Accommodation.Business.Interfaces;
using HotelLux.Accommodation.Business.Mappers;
using HotelLux.Accommodation.DataManagement.Interfaces;

namespace HotelLux.Accommodation.Business.Services;

public class SucursalImagenService : ISucursalImagenService
{
    private readonly ISucursalImagenDataService _dataService;
    private readonly ISucursalDataService _sucursalDataService;
    private readonly IAuditEmitter _audit;

    public SucursalImagenService(
        ISucursalImagenDataService dataService,
        ISucursalDataService sucursalDataService,
        IAuditEmitter audit)
    {
        _dataService = dataService;
        _sucursalDataService = sucursalDataService;
        _audit = audit;
    }

    public async Task<IReadOnlyList<SucursalImagenDTO>> ListarPorSucursalAsync(Guid sucursalGuid, CancellationToken ct = default)
    {
        var sucursal = await _sucursalDataService.ObtenerPorGuidAsync(sucursalGuid, ct);
        if (sucursal is null) throw new NotFoundException("Sucursal", sucursalGuid);
        var models = await _dataService.ListarPorSucursalAsync(sucursal.IdSucursal, ct);
        return models.Select(SucursalImagenBusinessMapper.ToDTO).ToList();
    }

    public async Task<SucursalImagenDTO> CrearAsync(Guid sucursalGuid, SucursalImagenCreateDTO dto, CancellationToken ct = default)
    {
        var sucursal = await _sucursalDataService.ObtenerPorGuidAsync(sucursalGuid, ct);
        if (sucursal is null) throw new NotFoundException("Sucursal", sucursalGuid);

        var dataModel = SucursalImagenBusinessMapper.ToDataModel(dto, sucursal.IdSucursal);
        var creado = await _dataService.CrearAsync(dataModel, ct);

        _audit.EmitFireAndForget("accommodation-service", "alojamiento.sucursal_imagen", "INSERT",
            creado.SucursalImagenGuid.ToString(), creado.IdSucursalImagen.ToString(),
            string.Empty, dto.CreadoPorUsuario ?? "api_user", null,
            null, JsonSerializer.Serialize(creado));

        return SucursalImagenBusinessMapper.ToDTO(creado);
    }

    public async Task EliminarAsync(Guid sucursalGuid, Guid imagenGuid, CancellationToken ct = default)
    {
        var sucursal = await _sucursalDataService.ObtenerPorGuidAsync(sucursalGuid, ct);
        if (sucursal is null) throw new NotFoundException("Sucursal", sucursalGuid);

        await _dataService.EliminarAsync(imagenGuid, ct);

        _audit.EmitFireAndForget("accommodation-service", "alojamiento.sucursal_imagen", "DELETE",
            imagenGuid.ToString(), sucursal.IdSucursal.ToString(),
            string.Empty, "api_user", null, null, null);
    }

    public async Task EliminarPorIdSucursalImagenAsync(Guid sucursalGuid, int idSucursalImagen, CancellationToken ct = default)
    {
        var sucursal = await _sucursalDataService.ObtenerPorGuidAsync(sucursalGuid, ct);
        if (sucursal is null) throw new NotFoundException("Sucursal", sucursalGuid);

        var models = await _dataService.ListarPorSucursalAsync(sucursal.IdSucursal, ct);
        var match = models.FirstOrDefault(m => m.IdSucursalImagen == idSucursalImagen)
            ?? throw new NotFoundException("Imagen sucursal", idSucursalImagen);

        await _dataService.EliminarAsync(match.SucursalImagenGuid, ct);

        _audit.EmitFireAndForget("accommodation-service", "alojamiento.sucursal_imagen", "DELETE",
            match.SucursalImagenGuid.ToString(), sucursal.IdSucursal.ToString(),
            string.Empty, "api_user", null, null, null);
    }
}
