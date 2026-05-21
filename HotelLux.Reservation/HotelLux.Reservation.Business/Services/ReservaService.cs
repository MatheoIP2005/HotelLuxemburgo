using System.Text.Json;
using Microsoft.Extensions.Logging;
using HotelLux.Reservation.Business.DTOs.Common;
using HotelLux.Reservation.Business.DTOs.Reserva;
using HotelLux.Reservation.Business.DTOs.ReservaHabitacion;
using HotelLux.Reservation.Business.Exceptions;
using HotelLux.Reservation.Business.Interfaces;
using HotelLux.Reservation.Business.Mappers;
using HotelLux.Reservation.Business.Validators;
using HotelLux.Reservation.DataManagement.Interfaces;
using HotelLux.Reservation.DataManagement.Models;

namespace HotelLux.Reservation.Business.Services;

public class ReservaService : IReservaService
{
    private readonly IReservaDataService _reservaDataService;
    private readonly IReservaHabitacionDataService _habitacionDataService;
    private readonly IClienteDataService _clienteDataService;
    private readonly IAccommodationClient _accommodationClient;
    private readonly IFinanceClient _finance;
    private readonly IAuditEmitter _audit;
    private readonly ILogger<ReservaService> _logger;

    public ReservaService(
        IReservaDataService reservaDataService,
        IReservaHabitacionDataService habitacionDataService,
        IClienteDataService clienteDataService,
        IAccommodationClient accommodationClient,
        IFinanceClient finance,
        IAuditEmitter audit,
        ILogger<ReservaService> logger)
    {
        _reservaDataService = reservaDataService;
        _habitacionDataService = habitacionDataService;
        _clienteDataService = clienteDataService;
        _accommodationClient = accommodationClient;
        _finance = finance;
        _audit = audit;
        _logger = logger;
    }

    public async Task<ReservaDTO> ObtenerPorGuidAsync(Guid reservaGuid, CancellationToken ct = default)
    {
        var m = await _reservaDataService.ObtenerPorGuidAsync(reservaGuid, ct);
        if (m is null) throw new NotFoundException("Reserva", reservaGuid);
        return ReservaBusinessMapper.ToDTO(m);
    }

    public async Task<IReadOnlyList<ReservaDTO>> ListarAsync(CancellationToken ct = default)
    {
        var list = await _reservaDataService.ListarAsync(ct);
        return list.Select(ReservaBusinessMapper.ToDTO).ToList();
    }

    public async Task<PagedResultDTO<ReservaDTO>> BuscarAsync(ReservaFiltroDTO filtro, CancellationToken ct = default)
    {
        var pagina = filtro.Pagina < 1 ? 1 : filtro.Pagina;
        var limite = filtro.Limite < 1 ? 20 : Math.Min(filtro.Limite, 200);

        var page = await _reservaDataService.BuscarAsync(
            filtro.ClienteGuid, filtro.SucursalGuid, filtro.EstadoReserva,
            filtro.FechaDesde, filtro.FechaHasta, filtro.OrigenCanal,
            pagina, limite, ct);

        return new PagedResultDTO<ReservaDTO>
        {
            Items = page.Items.Select(ReservaBusinessMapper.ToDTO).ToList(),
            Total = page.Total,
            Pagina = pagina,
            Limite = limite
        };
    }

    public async Task<IReadOnlyList<ReservaHabitacionDTO>> ListarHabitacionesAsync(Guid reservaGuid, CancellationToken ct = default)
    {
        var reserva = await _reservaDataService.ObtenerPorGuidAsync(reservaGuid, ct);
        if (reserva is null) throw new NotFoundException("Reserva", reservaGuid);

        var lines = await _habitacionDataService.ListarPorReservaAsync(reserva.IdReserva, ct);
        return lines.Select(ReservaHabitacionBusinessMapper.ToDTO).ToList();
    }

    public async Task<ReservaDTO> CrearAsync(ReservaCreateDTO dto, CancellationToken ct = default)
    {
        if ((!dto.ClienteGuid.HasValue || dto.ClienteGuid == Guid.Empty) && dto.Cliente is not null)
        {
            var ci = dto.Cliente;
            var existente = await _clienteDataService.ObtenerPorIdentificacionAsync(
                ci.TipoIdentificacion.Trim(), ci.NumeroIdentificacion.Trim(), ct);
            if (existente is not null)
            {
                dto.ClienteGuid = existente.ClienteGuid;
            }
            else
            {
                var nuevo = await _clienteDataService.CrearAsync(new ClienteDataModel
                {
                    TipoIdentificacion = ci.TipoIdentificacion.Trim(),
                    NumeroIdentificacion = ci.NumeroIdentificacion.Trim(),
                    Nombres = ci.Nombres.Trim(),
                    Apellidos = string.IsNullOrWhiteSpace(ci.Apellidos) ? null : ci.Apellidos.Trim(),
                    RazonSocial = null,
                    Correo = ci.Correo.Trim(),
                    Telefono = string.IsNullOrWhiteSpace(ci.Telefono) ? "-" : ci.Telefono.Trim(),
                    Direccion = string.IsNullOrWhiteSpace(ci.Direccion) ? "-" : ci.Direccion.Trim(),
                    CreadoPorUsuario = dto.CreadoPorUsuario ?? "portal_publico"
                }, ct);
                dto.ClienteGuid = nuevo.ClienteGuid;
            }
        }

        if (!dto.ClienteGuid.HasValue || dto.ClienteGuid == Guid.Empty)
            throw new ValidationException("Se requiere clienteGuid o un objeto cliente válido.",
                new[] { "Cliente requerido." });

        var errors = ReservaValidator.ValidarCreacion(dto);
        if (errors.Count > 0)
            throw new ValidationException("Datos de reserva inválidos.", errors.ToList());

        var model = ReservaBusinessMapper.ToDataModel(dto);
        var created = await _reservaDataService.CrearAsync(model, ct);

        _audit.EmitFireAndForget(
            "reservation-service",
            "reservas.reserva",
            "INSERT",
            created.ReservaGuid.ToString(),
            created.IdReserva.ToString(),
            Guid.Empty.ToString(),
            dto.CreadoPorUsuario ?? "api_user",
            dto.CreadoDesdeIp,
            null,
            JsonSerializer.Serialize(created));

        return ReservaBusinessMapper.ToDTO(created);
    }

    public async Task<ReservaDTO> ConfirmarAsync(Guid reservaGuid, string usuario, CancellationToken ct = default)
    {
        var m = await _reservaDataService.ObtenerPorGuidAsync(reservaGuid, ct);
        if (m is null) throw new NotFoundException("Reserva", reservaGuid);

        if (m.EstadoReserva == "CON")
            return ReservaBusinessMapper.ToDTO(m);

        if (m.EstadoReserva != "PEN")
            throw new ConflictException("Reserva", $"No se puede confirmar una reserva en estado {m.EstadoReserva}.");

        var lockedRooms = new List<Guid>();
        var committed = false;

        try
        {
            foreach (var hab in m.Habitaciones)
            {
                var ok = await _accommodationClient.ConfirmRoomLockAsync(
                    hab.HabitacionGuid, m.ReservaGuid,
                    hab.FechaInicio, hab.FechaFin, ct);

                if (!ok)
                    throw new ConflictException("Reserva",
                        $"No se pudo bloquear la habitación {hab.HabitacionGuid}. Verifique disponibilidad en alojamiento.");

                lockedRooms.Add(hab.HabitacionGuid);
            }

            foreach (var hab in m.Habitaciones)
            {
                await _habitacionDataService.ActualizarEstadoAsync(
                    hab.ReservaHabitacionGuid, "CON", usuario, ct);
            }

            m.EstadoReserva = "CON";
            m.FechaConfirmacionUtc = DateTimeOffset.UtcNow;
            m.ModificadoPorUsuario = usuario;
            m.FechaModificacionUtc = DateTimeOffset.UtcNow;

            var updated = await _reservaDataService.ActualizarAsync(m, ct)
                          ?? throw new InvalidOperationException("No se pudo actualizar la reserva tras confirmar.");

            _audit.EmitFireAndForget(
                "reservation-service",
                "reservas.reserva",
                "UPDATE",
                updated.ReservaGuid.ToString(),
                updated.IdReserva.ToString(),
                Guid.Empty.ToString(),
                usuario,
                null,
                JsonSerializer.Serialize(new { estado = "PEN" }),
                JsonSerializer.Serialize(new { estado = "CON", confirmado = true }));

            committed = true;
            _ = Task.Run(async () =>
            {
                try
                {
                    var lineas = BuildFinanceReservationLines(updated);
                    await _finance.GenerateReservationInvoiceAsync(
                        updated.ReservaGuid,
                        updated.ClienteGuid,
                        updated.SucursalGuid,
                        updated.SubtotalReserva,
                        updated.ValorIva,
                        updated.TotalReserva,
                        lineas,
                        usuario);
                }
                catch
                {
                    // La factura no debe revertir una reserva ya confirmada.
                }
            }, CancellationToken.None);
            return ReservaBusinessMapper.ToDTO(updated);
        }
        finally
        {
            if (!committed && lockedRooms.Count > 0)
                await ReleaseLocksAsync(m.ReservaGuid, lockedRooms, ct);
        }
    }

    public async Task<ReservaDTO> CancelarAsync(Guid reservaGuid, string motivo, string usuario, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(motivo))
            throw new ValidationException("Motivo obligatorio.", new[] { "Motivo es requerido para cancelar." });

        var m = await _reservaDataService.ObtenerPorGuidAsync(reservaGuid, ct);
        if (m is null) throw new NotFoundException("Reserva", reservaGuid);

        if (m.EstadoReserva == "CAN")
            return ReservaBusinessMapper.ToDTO(m);

        if (m.EstadoReserva is not ("PEN" or "CON"))
            throw new ConflictException("Reserva",
                $"Solo se pueden cancelar reservas pendientes o confirmadas (estado actual: {m.EstadoReserva}).");

        var estadoAnterior = m.EstadoReserva;

        if (m.EstadoReserva == "CON")
        {
            foreach (var hab in m.Habitaciones.Where(h => h.EstadoDetalle == "CON"))
                await _accommodationClient.ReleaseRoomLockAsync(hab.HabitacionGuid, m.ReservaGuid, ct);
        }

        foreach (var hab in m.Habitaciones)
        {
            await _habitacionDataService.ActualizarEstadoAsync(
                hab.ReservaHabitacionGuid, "CAN", usuario, ct);
        }

        m.EstadoReserva = "CAN";
        m.FechaCancelacionUtc = DateTimeOffset.UtcNow;
        m.MotivoCancelacion = motivo.Trim();
        m.ModificadoPorUsuario = usuario;
        m.FechaModificacionUtc = DateTimeOffset.UtcNow;

        var updated = await _reservaDataService.ActualizarAsync(m, ct)
                      ?? throw new InvalidOperationException("No se pudo cancelar la reserva.");

        _audit.EmitFireAndForget(
            "reservation-service",
            "reservas.reserva",
            "UPDATE",
            updated.ReservaGuid.ToString(),
            updated.IdReserva.ToString(),
            Guid.Empty.ToString(),
            usuario,
            null,
            JsonSerializer.Serialize(new { estado = estadoAnterior }),
            JsonSerializer.Serialize(new { estado = "CAN", motivo = motivo.Trim() }));

        return ReservaBusinessMapper.ToDTO(updated);
    }

    public async Task EliminarAsync(Guid reservaGuid, string usuario, CancellationToken ct = default)
    {
        var m = await _reservaDataService.ObtenerPorGuidAsync(reservaGuid, ct);
        if (m is null) throw new NotFoundException("Reserva", reservaGuid);

        if (m.EstadoReserva == "CON")
        {
            foreach (var hab in m.Habitaciones.Where(h => h.EstadoDetalle == "CON"))
                await _accommodationClient.ReleaseRoomLockAsync(hab.HabitacionGuid, m.ReservaGuid, ct);
        }

        var ok = await _reservaDataService.EliminarLogicoAsync(reservaGuid, usuario, ct);
        if (!ok) throw new NotFoundException("Reserva", reservaGuid);

        _audit.EmitFireAndForget(
            "reservation-service",
            "reservas.reserva",
            "DELETE",
            reservaGuid.ToString(),
            m.IdReserva.ToString(),
            Guid.Empty.ToString(),
            usuario,
            null,
            JsonSerializer.Serialize(m),
            null);
    }

    private async Task ReleaseLocksAsync(Guid reservaGuid, IReadOnlyList<Guid> habitacionGuids, CancellationToken ct)
    {
        foreach (var habGuid in habitacionGuids)
            await _accommodationClient.ReleaseRoomLockAsync(habGuid, reservaGuid, ct);
    }

    private static IEnumerable<(string Descripcion, decimal PrecioUnitario, decimal Cantidad, decimal Subtotal, decimal ValorIva, decimal Total)>
        BuildFinanceReservationLines(ReservaDataModel m)
    {
        return m.Habitaciones
            .Where(h => h.EstadoDetalle is "PEN" or "CON")
            .Select(h => (
                Descripcion: $"Habitación {h.HabitacionGuid} - {h.FechaInicio:d} a {h.FechaFin:d}",
                PrecioUnitario: h.PrecioNocheAplicado,
                Cantidad: (decimal)Math.Max(1, h.FechaFin.DayNumber - h.FechaInicio.DayNumber),
                Subtotal: h.SubtotalLinea,
                ValorIva: h.ValorIvaLinea,
                Total: h.TotalLinea
            ));
    }

    private async Task TrySincronizarFacturaReservaAsync(ReservaDataModel m, string usuario, CancellationToken ct)
    {
        if (m.EstadoReserva != "CON")
            return;
        try
        {
            var lineas = BuildFinanceReservationLines(m);
            var ok = await _finance.GenerateReservationInvoiceAsync(
                m.ReservaGuid,
                m.ClienteGuid,
                m.SucursalGuid,
                m.SubtotalReserva,
                m.ValorIva,
                m.TotalReserva,
                lineas,
                usuario,
                ct);
            if (!ok)
                _logger.LogWarning(
                    "Finance no pudo sincronizar factura RESERVA para reserva {ReservaGuid} (revisar mensaje en servicio Finance).",
                    m.ReservaGuid);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al sincronizar factura RESERVA para reserva {ReservaGuid}", m.ReservaGuid);
        }
    }

    public async Task<ReservaHabitacionDTO> AgregarHabitacionAsync(
        Guid reservaGuid, ReservaHabitacionCreateDTO dto, string usuario, CancellationToken ct = default)
    {
        var m = await _reservaDataService.ObtenerPorGuidAsync(reservaGuid, ct);
        if (m is null) throw new NotFoundException("Reserva", reservaGuid);

        if (m.EstadoReserva is "CAN")
            throw new ConflictException("Reserva", "No se pueden agregar habitaciones a una reserva cancelada.");

        if (m.EstadoReserva is not ("PEN" or "CON"))
            throw new ConflictException("Reserva", $"No se pueden agregar habitaciones en estado {m.EstadoReserva}.");

        var errs = ReservaValidator.ValidarLineaHabitacion(dto);
        if (errs.Count > 0)
            throw new ValidationException("Datos de línea inválidos.", errs.ToList());

        var estadoLinea = m.EstadoReserva == "CON" ? "CON" : "PEN";
        var line = ReservaHabitacionBusinessMapper.FromCreateDto(dto, usuario);
        line.EstadoDetalle = estadoLinea;

        var needsRelease = false;
        try
        {
            if (estadoLinea == "CON")
            {
                var ok = await _accommodationClient.ConfirmRoomLockAsync(
                    dto.HabitacionGuid, m.ReservaGuid, dto.FechaInicio, dto.FechaFin, ct);
                if (!ok)
                    throw new ConflictException("Reserva",
                        $"No se pudo bloquear la habitación {dto.HabitacionGuid}. Verifique disponibilidad en alojamiento.");
                needsRelease = true;
            }

            var inserted = await _habitacionDataService.InsertarLineaAsync(m.IdReserva, line, ct);
            await _reservaDataService.RecalcularTotalesDesdeHabitacionesAsync(reservaGuid, usuario, ct);
            needsRelease = false;

            var refreshed = await _reservaDataService.ObtenerPorGuidAsync(reservaGuid, ct)
                ?? throw new InvalidOperationException("Reserva no encontrada tras insertar línea.");
            await TrySincronizarFacturaReservaAsync(refreshed, usuario, ct);

            _audit.EmitFireAndForget(
                "reservation-service",
                "reservas.reserva_habitacion",
                "INSERT",
                inserted.ReservaHabitacionGuid.ToString(),
                m.IdReserva.ToString(),
                reservaGuid.ToString(),
                usuario,
                null,
                null,
                JsonSerializer.Serialize(inserted));

            return ReservaHabitacionBusinessMapper.ToDTO(inserted);
        }
        finally
        {
            if (needsRelease)
                await _accommodationClient.ReleaseRoomLockAsync(dto.HabitacionGuid, m.ReservaGuid, ct);
        }
    }

    public async Task EliminarHabitacionPorIdAsync(
        Guid reservaGuid, int idReservaHabitacion, string usuario, CancellationToken ct = default)
    {
        var m = await _reservaDataService.ObtenerPorGuidAsync(reservaGuid, ct);
        if (m is null) throw new NotFoundException("Reserva", reservaGuid);

        var line = m.Habitaciones.FirstOrDefault(h => h.IdReservaHabitacion == idReservaHabitacion);
        if (line is null)
            throw new NotFoundException("Línea de habitación", idReservaHabitacion.ToString());

        await EliminarHabitacionAsync(reservaGuid, line.ReservaHabitacionGuid, usuario, ct);
    }

    public async Task EliminarHabitacionAsync(
        Guid reservaGuid, Guid reservaHabitacionGuid, string usuario, CancellationToken ct = default)
    {
        var m = await _reservaDataService.ObtenerPorGuidAsync(reservaGuid, ct);
        if (m is null) throw new NotFoundException("Reserva", reservaGuid);

        if (m.EstadoReserva is "CAN")
            throw new ConflictException("Reserva", "No se pueden eliminar líneas de una reserva cancelada.");

        var line = m.Habitaciones.FirstOrDefault(h => h.ReservaHabitacionGuid == reservaHabitacionGuid);
        if (line is null)
            throw new NotFoundException("Línea de habitación", reservaHabitacionGuid);

        if (line.EstadoDetalle == "CAN")
            throw new ConflictException("Línea de habitación", "La línea ya está cancelada y no puede eliminarse.");

        var activas = m.Habitaciones.Count(h => h.EstadoDetalle is "PEN" or "CON");
        if (activas <= 1)
            throw new ValidationException("Debe permanecer al menos una línea de habitación activa.",
                new[] { "No se puede eliminar la única línea activa." });

        if (line.EstadoDetalle == "CON")
            await _accommodationClient.ReleaseRoomLockAsync(line.HabitacionGuid, m.ReservaGuid, ct);

        var okDel = await _habitacionDataService.EliminarLineaAsync(m.IdReserva, reservaHabitacionGuid, ct);
        if (!okDel)
            throw new NotFoundException("Línea de habitación", reservaHabitacionGuid);

        await _reservaDataService.RecalcularTotalesDesdeHabitacionesAsync(reservaGuid, usuario, ct);

        var refreshed = await _reservaDataService.ObtenerPorGuidAsync(reservaGuid, ct)
            ?? throw new InvalidOperationException("Reserva no encontrada tras eliminar línea.");
        await TrySincronizarFacturaReservaAsync(refreshed, usuario, ct);

        _audit.EmitFireAndForget(
            "reservation-service",
            "reservas.reserva_habitacion",
            "DELETE",
            reservaHabitacionGuid.ToString(),
            m.IdReserva.ToString(),
            reservaGuid.ToString(),
            usuario,
            null,
            JsonSerializer.Serialize(line),
            null);
    }
}
