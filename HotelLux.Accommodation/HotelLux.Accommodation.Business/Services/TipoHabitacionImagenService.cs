using System.Text.Json;
using HotelLux.Accommodation.Business.DTOs.TipoHabitacionImagen;
using HotelLux.Accommodation.Business.Exceptions;
using HotelLux.Accommodation.Business.Interfaces;
using HotelLux.Accommodation.Business.Mappers;
using HotelLux.Accommodation.DataManagement.Interfaces;

namespace HotelLux.Accommodation.Business.Services;

public class TipoHabitacionImagenService : ITipoHabitacionImagenService
{
    private readonly ITipoHabitacionImagenDataService _dataService;
    private readonly ITipoHabitacionDataService _tipoDataService;
    private readonly IAuditEmitter _audit;

    public TipoHabitacionImagenService(
        ITipoHabitacionImagenDataService dataService,
        ITipoHabitacionDataService tipoDataService,
        IAuditEmitter audit)
    {
        _dataService = dataService;
        _tipoDataService = tipoDataService;
        _audit = audit;
    }

    public async Task<IReadOnlyList<TipoHabitacionImagenDTO>> ListarPorTipoAsync(Guid tipoGuid, CancellationToken ct = default)
    {
        var tipo = await _tipoDataService.ObtenerPorGuidAsync(tipoGuid, ct);
        if (tipo is null) throw new NotFoundException("TipoHabitación", tipoGuid);
        var models = await _dataService.ListarPorTipoAsync(tipoGuid, ct);
        return models.Select(TipoHabitacionImagenBusinessMapper.ToDTO).ToList();
    }

    public async Task<TipoHabitacionImagenDTO> CrearAsync(Guid tipoGuid, TipoHabitacionImagenCreateDTO dto, CancellationToken ct = default)
    {
        var tipo = await _tipoDataService.ObtenerPorGuidAsync(tipoGuid, ct);
        if (tipo is null) throw new NotFoundException("TipoHabitación", tipoGuid);

        var dataModel = TipoHabitacionImagenBusinessMapper.ToDataModel(dto, tipo.IdTipoHabitacion);
        var creado = await _dataService.CrearAsync(dataModel, ct);

        _audit.EmitFireAndForget("accommodation-service", "alojamiento.tipo_habitacion_imagen", "INSERT",
            tipoGuid.ToString(), creado.IdTipoHabitacionImagen.ToString(),
            string.Empty, dto.CreadoPorUsuario ?? "api_user", null,
            null, JsonSerializer.Serialize(creado));

        return TipoHabitacionImagenBusinessMapper.ToDTO(creado);
    }

    public async Task EliminarAsync(Guid tipoGuid, int idImagen, CancellationToken ct = default)
    {
        var tipo = await _tipoDataService.ObtenerPorGuidAsync(tipoGuid, ct);
        if (tipo is null) throw new NotFoundException("TipoHabitación", tipoGuid);

        await _dataService.EliminarAsync(tipoGuid, idImagen, ct);

        _audit.EmitFireAndForget("accommodation-service", "alojamiento.tipo_habitacion_imagen", "DELETE",
            tipoGuid.ToString(), idImagen.ToString(),
            string.Empty, "api_user", null, null, null);
    }
}
