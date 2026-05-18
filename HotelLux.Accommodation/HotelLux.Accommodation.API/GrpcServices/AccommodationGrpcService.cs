using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using HotelLux.Accommodation.Business.Interfaces;
using HotelLux.Accommodation.DataManagement.Interfaces;
using HotelLux.Protos.Accommodation;
using Microsoft.Extensions.Logging;

namespace HotelLux.Accommodation.API.GrpcServices;

public class AccommodationGrpcService : AccommodationService.AccommodationServiceBase
{
    private readonly IHabitacionDataService _habitacionDataService;
    private readonly ITarifaDataService _tarifaDataService;
    private readonly IAuditEmitter _audit;
    private readonly ILogger<AccommodationGrpcService> _logger;

    private static readonly HashSet<string> EstadosValidos =
        new(StringComparer.Ordinal) { "DIS", "OCU", "MNT", "FDS", "INA" };

    public AccommodationGrpcService(
        IHabitacionDataService habitacionDataService,
        ITarifaDataService tarifaDataService,
        IAuditEmitter audit,
        ILogger<AccommodationGrpcService> logger)
    {
        _habitacionDataService = habitacionDataService;
        _tarifaDataService = tarifaDataService;
        _audit = audit;
        _logger = logger;
    }

    // ----------------------------------------------------------------
    // CheckAvailability
    // Retorna todas las habitaciones físicas con EstadoHabitacion=="DIS"
    // que cumplen capacidad mínima y, opcionalmente, tipo de habitación.
    // Llamado por HotelLux.Reservation antes de crear una reserva.
    // ----------------------------------------------------------------
    public override async Task<CheckAvailabilityResponse> CheckAvailability(
        CheckAvailabilityRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.SucursalGuid, out var sucursalGuid))
        {
            _logger.LogWarning("CheckAvailability: SucursalGuid inválido '{Raw}'", request.SucursalGuid);
            return new CheckAvailabilityResponse { Disponible = false };
        }

        var inicio = DateOnly.FromDateTime(request.FechaEntrada.ToDateTime());
        var fin = DateOnly.FromDateTime(request.FechaSalida.ToDateTime());

        var habitaciones = await _habitacionDataService
            .ListarDisponiblesAsync(sucursalGuid, inicio, fin, context.CancellationToken);

        if (request.CantidadPersonas > 0)
            habitaciones = habitaciones
                .Where(h => h.CapacidadHabitacion >= request.CantidadPersonas)
                .ToList();

        if (Guid.TryParse(request.TipoHabitacionGuid, out var tipoGuid) && tipoGuid != Guid.Empty)
            habitaciones = habitaciones
                .Where(h => h.TipoHabitacionGuid == tipoGuid)
                .ToList();

        if (!habitaciones.Any())
            return new CheckAvailabilityResponse { Disponible = false };

        var response = new CheckAvailabilityResponse { Disponible = true };

        foreach (var h in habitaciones)
        {
            response.Habitaciones.Add(new HabitacionDisponible
            {
                HabitacionGuid = h.HabitacionGuid.ToString(),
                TipoHabitacionGuid = h.TipoHabitacionGuid.ToString(),
                NumeroHabitacion = h.NumeroHabitacion,
                Capacidad = h.CapacidadHabitacion,
                PrecioNoche = (double)h.PrecioBase
            });
        }

        _logger.LogInformation(
            "CheckAvailability: sucursal={SucursalGuid} {Inicio}/{Fin} → {Cantidad} habitaciones disponibles",
            sucursalGuid, inicio, fin, response.Habitaciones.Count);

        return response;
    }

    // ----------------------------------------------------------------
    // ConfirmRoomLock
    // Cambia el estado de la habitación a "OCU".
    // Llamado por HotelLux.Reservation al confirmar una reserva.
    // No es idempotente: rechaza si el estado actual no es "DIS".
    // ----------------------------------------------------------------
    public override async Task<ConfirmRoomLockResponse> ConfirmRoomLock(
        ConfirmRoomLockRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.HabitacionGuid, out var habitacionGuid))
        {
            _logger.LogWarning("ConfirmRoomLock: HabitacionGuid inválido '{Raw}'", request.HabitacionGuid);
            return new ConfirmRoomLockResponse
            {
                Success = false,
                Mensaje = $"GUID de habitación inválido: '{request.HabitacionGuid}'"
            };
        }

        var habitacion = await _habitacionDataService
            .ObtenerPorGuidAsync(habitacionGuid, context.CancellationToken);

        if (habitacion is null)
        {
            _logger.LogWarning("ConfirmRoomLock: habitación {Guid} no encontrada", habitacionGuid);
            return new ConfirmRoomLockResponse
            {
                Success = false,
                Mensaje = $"Habitación {habitacionGuid} no encontrada."
            };
        }

        if (habitacion.EstadoHabitacion != "DIS")
        {
            _logger.LogWarning(
                "ConfirmRoomLock: habitación {Guid} no está disponible (estado={Estado})",
                habitacionGuid, habitacion.EstadoHabitacion);
            return new ConfirmRoomLockResponse
            {
                Success = false,
                Mensaje = $"Habitación {habitacion.NumeroHabitacion} no está disponible " +
                          $"(estado actual: {habitacion.EstadoHabitacion})."
            };
        }

        var estadoAnterior = habitacion.EstadoHabitacion;

        await _habitacionDataService.CambiarEstadoAsync(
            habitacionGuid, "OCU", "grpc_reservation", context.CancellationToken);

        _audit.EmitFireAndForget(
            "accommodation-service",
            "alojamiento.habitacion",
            "LOCK",
            habitacionGuid.ToString(),
            habitacion.IdHabitacion.ToString(),
            Guid.Empty.ToString(),
            "grpc_reservation",
            null,
            System.Text.Json.JsonSerializer.Serialize(new { estado = estadoAnterior }),
            System.Text.Json.JsonSerializer.Serialize(new
            {
                estado = "OCU",
                reserva_guid = request.ReservaGuid
            }));

        _logger.LogInformation(
            "ConfirmRoomLock: habitación {NumHab} ({Guid}) → OCU para reserva {ReservaGuid}",
            habitacion.NumeroHabitacion, habitacionGuid, request.ReservaGuid);

        return new ConfirmRoomLockResponse
        {
            Success = true,
            Mensaje = $"Habitación {habitacion.NumeroHabitacion} bloqueada correctamente."
        };
    }

    // ----------------------------------------------------------------
    // ReleaseRoomLock
    // Revierte el estado de la habitación a "DIS".
    // Operación de compensación saga — es idempotente y nunca lanza excepción.
    // ----------------------------------------------------------------
    public override async Task<ReleaseRoomLockResponse> ReleaseRoomLock(
        ReleaseRoomLockRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.HabitacionGuid, out var habitacionGuid))
        {
            _logger.LogWarning("ReleaseRoomLock: HabitacionGuid inválido '{Raw}'", request.HabitacionGuid);
            return new ReleaseRoomLockResponse
            {
                Success = false,
                Mensaje = $"GUID de habitación inválido: '{request.HabitacionGuid}'"
            };
        }

        var habitacion = await _habitacionDataService
            .ObtenerPorGuidAsync(habitacionGuid, context.CancellationToken);

        if (habitacion is null)
        {
            _logger.LogWarning(
                "ReleaseRoomLock: habitación {Guid} no encontrada — compensación ignorada", habitacionGuid);
            return new ReleaseRoomLockResponse
            {
                Success = false,
                Mensaje = $"Habitación {habitacionGuid} no encontrada."
            };
        }

        if (habitacion.EstadoHabitacion == "DIS")
        {
            _logger.LogInformation(
                "ReleaseRoomLock: habitación {Guid} ya en DIS — compensación idempotente", habitacionGuid);
            return new ReleaseRoomLockResponse
            {
                Success = true,
                Mensaje = "Habitación ya estaba disponible."
            };
        }

        var estadoAnterior = habitacion.EstadoHabitacion;

        await _habitacionDataService.CambiarEstadoAsync(
            habitacionGuid, "DIS", "grpc_reservation_saga", context.CancellationToken);

        _audit.EmitFireAndForget(
            "accommodation-service",
            "alojamiento.habitacion",
            "RELEASE",
            habitacionGuid.ToString(),
            habitacion.IdHabitacion.ToString(),
            Guid.Empty.ToString(),
            "grpc_reservation_saga",
            null,
            System.Text.Json.JsonSerializer.Serialize(new
            {
                estado = estadoAnterior,
                reserva_guid = request.ReservaGuid
            }),
            System.Text.Json.JsonSerializer.Serialize(new
            {
                estado = "DIS",
                reserva_guid = request.ReservaGuid,
                compensacion = true
            }));

        _logger.LogInformation(
            "ReleaseRoomLock: habitación {NumHab} ({Guid}) {EstadoAnterior}→DIS (saga) reserva={ReservaGuid}",
            habitacion.NumeroHabitacion, habitacionGuid, estadoAnterior, request.ReservaGuid);

        return new ReleaseRoomLockResponse
        {
            Success = true,
            Mensaje = $"Habitación {habitacion.NumeroHabitacion} liberada correctamente."
        };
    }

    // ----------------------------------------------------------------
    // UpdateRoomStatus
    // Cambio genérico de estado. Llamado por HotelLux.Stay en
    // check-in (DIS→OCU) y check-out (OCU→DIS), o por mantenimiento.
    // ----------------------------------------------------------------
    public override async Task<UpdateRoomStatusResponse> UpdateRoomStatus(
        UpdateRoomStatusRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.HabitacionGuid, out var habitacionGuid))
        {
            _logger.LogWarning("UpdateRoomStatus: HabitacionGuid inválido '{Raw}'", request.HabitacionGuid);
            return new UpdateRoomStatusResponse
            {
                Success = false,
                Mensaje = $"GUID de habitación inválido: '{request.HabitacionGuid}'"
            };
        }

        var nuevoEstado = request.NuevoEstado?.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(nuevoEstado) || !EstadosValidos.Contains(nuevoEstado))
        {
            _logger.LogWarning(
                "UpdateRoomStatus: estado '{Estado}' no válido para habitación {Guid}",
                request.NuevoEstado, habitacionGuid);
            return new UpdateRoomStatusResponse
            {
                Success = false,
                Mensaje = $"Estado '{request.NuevoEstado}' no válido. " +
                          "Valores permitidos: DIS, OCU, MNT, FDS, INA."
            };
        }

        var habitacion = await _habitacionDataService
            .ObtenerPorGuidAsync(habitacionGuid, context.CancellationToken);

        if (habitacion is null)
        {
            _logger.LogWarning("UpdateRoomStatus: habitación {Guid} no encontrada", habitacionGuid);
            return new UpdateRoomStatusResponse
            {
                Success = false,
                Mensaje = $"Habitación {habitacionGuid} no encontrada."
            };
        }

        if (habitacion.EstadoHabitacion == nuevoEstado)
        {
            return new UpdateRoomStatusResponse
            {
                Success = true,
                Mensaje = $"Habitación {habitacion.NumeroHabitacion} ya tenía estado {nuevoEstado}."
            };
        }

        var estadoAnterior = habitacion.EstadoHabitacion;

        await _habitacionDataService.CambiarEstadoAsync(
            habitacionGuid, nuevoEstado, "grpc_stay", context.CancellationToken);

        _audit.EmitFireAndForget(
            "accommodation-service",
            "alojamiento.habitacion",
            "UPDATE_STATUS",
            habitacionGuid.ToString(),
            habitacion.IdHabitacion.ToString(),
            Guid.Empty.ToString(),
            "grpc_stay",
            null,
            System.Text.Json.JsonSerializer.Serialize(new { estado = estadoAnterior }),
            System.Text.Json.JsonSerializer.Serialize(new
            {
                estado = nuevoEstado,
                operacion_guid = request.OperacionGuid
            }));

        _logger.LogInformation(
            "UpdateRoomStatus: habitación {NumHab} ({Guid}) {Anterior}→{Nuevo} por operacion={OpGuid}",
            habitacion.NumeroHabitacion, habitacionGuid, estadoAnterior, nuevoEstado, request.OperacionGuid);

        return new UpdateRoomStatusResponse
        {
            Success = true,
            Mensaje = $"Habitación {habitacion.NumeroHabitacion} actualizada a {nuevoEstado}."
        };
    }

    // ----------------------------------------------------------------
    // FindAvailableRoomByType
    // Dado un tipo de habitación y rango de fechas, devuelve la primera
    // habitación física disponible junto con la tarifa activa de mayor
    // prioridad (Prioridad más bajo = mayor prioridad) que cubra el rango.
    // Usado por HotelLux.Reservation cuando el booking envía
    // tipo_habitacion_guid (sin especificar habitación física concreta).
    // ----------------------------------------------------------------
    public override async Task<FindAvailableRoomByTypeResponse> FindAvailableRoomByType(
        FindAvailableRoomByTypeRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.SucursalGuid, out var sucursalGuid))
        {
            _logger.LogWarning(
                "FindAvailableRoomByType: SucursalGuid inválido '{Raw}'", request.SucursalGuid);
            return new FindAvailableRoomByTypeResponse
            {
                Encontrada = false,
                Mensaje = $"GUID de sucursal inválido: '{request.SucursalGuid}'"
            };
        }

        if (!Guid.TryParse(request.TipoHabitacionGuid, out var tipoGuid))
        {
            _logger.LogWarning(
                "FindAvailableRoomByType: TipoHabitacionGuid inválido '{Raw}'", request.TipoHabitacionGuid);
            return new FindAvailableRoomByTypeResponse
            {
                Encontrada = false,
                Mensaje = $"GUID de tipo de habitación inválido: '{request.TipoHabitacionGuid}'"
            };
        }

        var inicio = DateOnly.FromDateTime(request.FechaInicio.ToDateTime());
        var fin = DateOnly.FromDateTime(request.FechaFin.ToDateTime());

        var disponibles = await _habitacionDataService
            .ListarDisponiblesAsync(sucursalGuid, inicio, fin, context.CancellationToken);

        var habitacion = disponibles
            .Where(h => h.TipoHabitacionGuid == tipoGuid)
            .OrderBy(h => h.NumeroHabitacion)
            .FirstOrDefault();

        if (habitacion is null)
        {
            _logger.LogInformation(
                "FindAvailableRoomByType: sin disponibilidad tipo={TipoGuid} sucursal={SucursalGuid} [{Inicio}/{Fin}]",
                tipoGuid, sucursalGuid, inicio, fin);
            return new FindAvailableRoomByTypeResponse
            {
                Encontrada = false,
                Mensaje = "No hay habitaciones disponibles para el tipo y fechas solicitados."
            };
        }

        var tarifas = await _tarifaDataService
            .ListarPorSucursalAsync(sucursalGuid, context.CancellationToken);

        var tarifa = tarifas
            .Where(t =>
                t.TipoHabitacionGuid == tipoGuid &&
                t.EstadoTarifa == "ACT" &&
                !t.EsEliminado &&
                t.FechaInicio <= inicio &&
                t.FechaFin >= fin)
            .OrderBy(t => t.Prioridad)
            .FirstOrDefault();

        var precioPorNoche = tarifa is not null
            ? (double)tarifa.PrecioPorNoche
            : (double)habitacion.PrecioBase;

        var tarifaGuid = tarifa?.TarifaGuid.ToString() ?? string.Empty;

        _logger.LogInformation(
            "FindAvailableRoomByType: habitación {NumHab} ({HabGuid}) tipo={TipoGuid} " +
            "precio={Precio} tarifa={TarifaGuid}",
            habitacion.NumeroHabitacion, habitacion.HabitacionGuid,
            tipoGuid, precioPorNoche, tarifaGuid);

        return new FindAvailableRoomByTypeResponse
        {
            Encontrada = true,
            HabitacionGuid = habitacion.HabitacionGuid.ToString(),
            PrecioNoche = precioPorNoche,
            TarifaGuid = tarifaGuid,
            Mensaje = string.Empty
        };
    }
}
