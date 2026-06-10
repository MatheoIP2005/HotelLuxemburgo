using System.Text.Json;
using HotelLux.Accommodation.Business.DTOs.Habitacion;
using HotelLux.Accommodation.Business.Exceptions;
using HotelLux.Accommodation.Business.Interfaces;
using HotelLux.Accommodation.Business.Mappers;
using HotelLux.Accommodation.Business.Validators;
using HotelLux.Accommodation.DataManagement.Interfaces;

namespace HotelLux.Accommodation.Business.Services;

public class HabitacionService : IHabitacionService
{
    private readonly IHabitacionDataService _dataService;
    private readonly ISucursalDataService _sucursalDataService;
    private readonly ITipoHabitacionDataService _tipoDataService;
    private readonly IAuditEmitter _audit;

    public HabitacionService(
        IHabitacionDataService dataService,
        ISucursalDataService sucursalDataService,
        ITipoHabitacionDataService tipoDataService,
        IAuditEmitter audit)
    {
        _dataService = dataService;
        _sucursalDataService = sucursalDataService;
        _tipoDataService = tipoDataService;
        _audit = audit;
    }

    public async Task<HabitacionDTO> ObtenerPorGuidAsync(Guid guid, CancellationToken ct = default)
    {
        var model = await _dataService.ObtenerPorGuidAsync(guid, ct);
        if (model is null) throw new NotFoundException("Habitación", guid);
        return HabitacionBusinessMapper.ToDTO(model);
    }

    public async Task<IReadOnlyList<HabitacionDTO>> ListarAsync(CancellationToken ct = default)
    {
        var models = await _dataService.ListarAsync(ct);
        return models.Select(HabitacionBusinessMapper.ToDTO).ToList();
    }

    public async Task<IReadOnlyList<HabitacionDTO>> ListarPorSucursalAsync(Guid sucursalGuid, CancellationToken ct = default)
    {
        var sucursal = await _sucursalDataService.ObtenerPorGuidAsync(sucursalGuid, ct);
        if (sucursal is null || sucursal.EsEliminado || sucursal.EstadoSucursal != "ACT")
            throw new NotFoundException("Sucursal", sucursalGuid);

        var models = await _dataService.ListarPorSucursalAsync(sucursalGuid, ct);
        return models.Select(HabitacionBusinessMapper.ToDTO).ToList();
    }

    public async Task<IReadOnlyList<HabitacionDTO>> ListarDisponiblesAsync(
        Guid sucursalGuid, DateOnly fechaInicio, DateOnly fechaFin, CancellationToken ct = default)
    {
        var sucursal = await _sucursalDataService.ObtenerPorGuidAsync(sucursalGuid, ct);
        if (sucursal is null || sucursal.EsEliminado || sucursal.EstadoSucursal != "ACT")
            throw new NotFoundException("Sucursal", sucursalGuid);

        var models = await _dataService.ListarDisponiblesAsync(sucursalGuid, fechaInicio, fechaFin, ct);
        return models.Select(HabitacionBusinessMapper.ToDTO).ToList();
    }

    public async Task<HabitacionDTO> CrearAsync(HabitacionCreateDTO dto, CancellationToken ct = default)
    {
        var errors = HabitacionValidator.ValidarCreacion(dto);
        if (errors.Count != 0) throw new ValidationException("Solicitud de creación inválida.", errors);

        var sucursal = await _sucursalDataService.ObtenerPorGuidAsync(dto.SucursalGuid, ct);
        if (sucursal is null) throw new NotFoundException("Sucursal", dto.SucursalGuid);

        var tipo = await _tipoDataService.ObtenerPorGuidAsync(dto.TipoHabitacionGuid, ct);
        if (tipo is null) throw new NotFoundException("TipoHabitación", dto.TipoHabitacionGuid);

        var dataModel = HabitacionBusinessMapper.ToDataModel(dto, sucursal.IdSucursal, tipo.IdTipoHabitacion);
        var creado = await _dataService.CrearAsync(dataModel, ct);

        _audit.EmitFireAndForget("accommodation-service", "alojamiento.habitacion", "INSERT",
            creado.HabitacionGuid.ToString(), creado.IdHabitacion.ToString(),
            string.Empty, dto.CreadoPorUsuario ?? "api_user", dto.CreadoDesdeIp,
            null, JsonSerializer.Serialize(creado));

        return HabitacionBusinessMapper.ToDTO(creado);
    }

    public async Task<HabitacionDTO> ActualizarAsync(Guid guid, HabitacionUpdateDTO dto, CancellationToken ct = default)
    {
        var errors = HabitacionValidator.ValidarActualizacion(dto);
        if (errors.Count != 0) throw new ValidationException("Solicitud de actualización inválida.", errors);

        var existente = await _dataService.ObtenerPorGuidAsync(guid, ct);
        if (existente is null) throw new NotFoundException("Habitación", guid);

        var anterior = JsonSerializer.Serialize(existente);
        var dataModel = HabitacionBusinessMapper.ToDataModel(dto, existente);
        var actualizado = await _dataService.ActualizarAsync(dataModel, ct);

        _audit.EmitFireAndForget("accommodation-service", "alojamiento.habitacion", "UPDATE",
            guid.ToString(), existente.IdHabitacion.ToString(),
            string.Empty, dto.ModificadoPorUsuario ?? "api_user", dto.ModificadoDesdeIp,
            anterior, JsonSerializer.Serialize(actualizado));

        return HabitacionBusinessMapper.ToDTO(actualizado!);
    }

    public async Task CambiarEstadoAsync(Guid guid, string nuevoEstado, string usuario, CancellationToken ct = default)
    {
        var validos = new[] { "DIS", "OCU", "MNT", "FDS", "INA" };
        if (!validos.Contains(nuevoEstado))
            throw new ValidationException("Estado inválido.", new[] { $"Estado '{nuevoEstado}' no es válido. Valores: DIS, OCU, MNT, FDS, INA" });

        var existente = await _dataService.ObtenerPorGuidAsync(guid, ct);
        if (existente is null) throw new NotFoundException("Habitación", guid);

        await _dataService.CambiarEstadoAsync(guid, nuevoEstado, usuario, ct);

        _audit.EmitFireAndForget("accommodation-service", "alojamiento.habitacion", "UPDATE",
            guid.ToString(), existente.IdHabitacion.ToString(),
            string.Empty, usuario, null,
            $"{{\"estado\":\"{existente.EstadoHabitacion}\"}}",
            $"{{\"estado\":\"{nuevoEstado}\"}}");
    }

    public async Task InhabilitarAsync(Guid guid, string usuario, CancellationToken ct = default)
    {
        var existente = await _dataService.ObtenerPorGuidAsync(guid, ct);
        if (existente is null) throw new NotFoundException("Habitación", guid);

        existente.EstadoHabitacion = "INA";
        existente.FechaInhabilitacionUtc = DateTimeOffset.UtcNow;
        existente.MotivoInhabilitacion = $"Inhabilitada por {usuario}";
        existente.ModificadoPorUsuario = usuario;
        existente.FechaModificacionUtc = DateTimeOffset.UtcNow;
        await _dataService.ActualizarAsync(existente, ct);

        _audit.EmitFireAndForget("accommodation-service", "alojamiento.habitacion", "UPDATE",
            guid.ToString(), existente.IdHabitacion.ToString(),
            string.Empty, usuario, null, null, null);
    }

    public async Task EliminarAsync(Guid guid, string usuario, CancellationToken ct = default)
    {
        var existente = await _dataService.ObtenerPorGuidAsync(guid, ct);
        if (existente is null) throw new NotFoundException("Habitación", guid);

        await _dataService.EliminarLogicoAsync(existente.IdHabitacion, usuario, ct);

        _audit.EmitFireAndForget("accommodation-service", "alojamiento.habitacion", "DELETE",
            guid.ToString(), existente.IdHabitacion.ToString(),
            string.Empty, usuario, null,
            JsonSerializer.Serialize(existente), null);
    }

    public async Task<HabitacionBloqueoResult> ConfirmarBloqueoReservaAsync(
        Guid habitacionGuid,
        Guid reservaGuid,
        CancellationToken ct = default)
    {
        var habitacion = await _dataService.ObtenerPorGuidAsync(habitacionGuid, ct);
        if (habitacion is null)
        {
            return new HabitacionBloqueoResult(
                false,
                $"Habitación {habitacionGuid} no encontrada.");
        }

        if (habitacion.EstadoHabitacion == "OCU")
        {
            return new HabitacionBloqueoResult(
                true,
                $"Habitación {habitacion.NumeroHabitacion} ya estaba bloqueada (OCU).");
        }

        if (habitacion.EstadoHabitacion != "DIS")
        {
            return new HabitacionBloqueoResult(
                false,
                $"Habitación {habitacion.NumeroHabitacion} no está disponible " +
                $"(estado actual: {habitacion.EstadoHabitacion}). " +
                "Libérela en alojamiento (estado DIS) o elija otra habitación.");
        }

        await _dataService.CambiarEstadoAsync(
            habitacionGuid,
            "OCU",
            "reservation-lock",
            ct);

        _audit.EmitFireAndForget(
            "accommodation-service",
            "alojamiento.habitacion",
            "LOCK",
            habitacionGuid.ToString(),
            habitacion.IdHabitacion.ToString(),
            reservaGuid.ToString(),
            "reservation-lock",
            null,
            JsonSerializer.Serialize(new { estado = "DIS" }),
            JsonSerializer.Serialize(new { estado = "OCU", reserva_guid = reservaGuid }));

        return new HabitacionBloqueoResult(
            true,
            $"Habitación {habitacion.NumeroHabitacion} bloqueada correctamente.");
    }

    public async Task<HabitacionBloqueoResult> LiberarBloqueoReservaAsync(
        Guid habitacionGuid,
        Guid reservaGuid,
        CancellationToken ct = default)
    {
        var habitacion = await _dataService.ObtenerPorGuidAsync(habitacionGuid, ct);
        if (habitacion is null)
        {
            return new HabitacionBloqueoResult(
                false,
                $"Habitación {habitacionGuid} no encontrada.");
        }

        if (habitacion.EstadoHabitacion == "DIS")
        {
            return new HabitacionBloqueoResult(true, "Habitación ya estaba disponible.");
        }

        await _dataService.CambiarEstadoAsync(
            habitacionGuid,
            "DIS",
            "reservation-saga",
            ct);

        return new HabitacionBloqueoResult(
            true,
            $"Habitación {habitacion.NumeroHabitacion} liberada correctamente.");
    }
}
