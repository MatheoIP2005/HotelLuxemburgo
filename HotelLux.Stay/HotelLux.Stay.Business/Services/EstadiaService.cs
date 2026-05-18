using System.Text.Json;
using HotelLux.Stay.Business.DTOs;
using HotelLux.Stay.Business.Exceptions;
using HotelLux.Stay.Business.Interfaces;
using HotelLux.Stay.DataManagement.Interfaces;
using HotelLux.Stay.DataManagement.Models;

namespace HotelLux.Stay.Business.Services;

public class EstadiaService : IEstadiaService
{
    private readonly IEstadiaDataService _estadiaData;
    private readonly IReservationStayClient _reservation;
    private readonly IAccommodationStayClient _accommodation;
    private readonly IFinanceStayClient _finance;
    private readonly IAuditEmitter _audit;

    public EstadiaService(
        IEstadiaDataService estadiaData,
        IReservationStayClient reservation,
        IAccommodationStayClient accommodation,
        IFinanceStayClient finance,
        IAuditEmitter audit)
    {
        _estadiaData = estadiaData;
        _reservation = reservation;
        _accommodation = accommodation;
        _finance = finance;
        _audit = audit;
    }

    public async Task<EstadiaDto> CheckInAsync(CheckInDto dto, CancellationToken ct = default)
    {
        if (dto.ReservaGuid == Guid.Empty)
            throw new ValidationException("ReservaGuid inválido.", new[] { "ReservaGuid es obligatorio." });

        var val = await _reservation.ValidarReservaParaCheckinAsync(dto.ReservaGuid, ct);
        if (!val.Valid)
            throw new ValidationException(val.Mensaje, new[] { val.Mensaje });

        ReservaHabitacionValidada? linea = null;
        if (dto.ReservaHabitacionGuid.HasValue && dto.ReservaHabitacionGuid.Value != Guid.Empty)
        {
            linea = val.Lineas.FirstOrDefault(l => l.ReservaHabitacionGuid == dto.ReservaHabitacionGuid.Value);
            if (linea is null)
                throw new ValidationException("Línea de reserva no válida para check-in.",
                    new[] { "ReservaHabitacionGuid no coincide con la reserva validada." });
        }
        else
        {
            linea = val.Lineas.FirstOrDefault();
            if (linea is null)
                throw new ValidationException("Reserva sin líneas de habitación.", Array.Empty<string>());
        }

        var activaLinea = await _estadiaData.ObtenerActivaPorReservaHabitacionGuidAsync(linea.ReservaHabitacionGuid, ct);
        if (activaLinea is not null)
            throw new ConflictException("Estadía", "Ya existe estadía activa para esta línea de habitación.");

        var estadiaGuid = Guid.NewGuid();
        var okRoom = await _accommodation.UpdateRoomStatusAsync(
            linea.HabitacionGuid, "OCU", estadiaGuid, ct);
        if (!okRoom)
            throw new ConflictException("Habitación",
                "No se pudo actualizar el estado de la habitación en alojamiento (check-in).");

        var usuario = dto.CreadoPorUsuario ?? "stay_api";
        var model = new EstadiaDataModel
        {
            EstadiaGuid = estadiaGuid,
            ReservaGuid = dto.ReservaGuid,
            ReservaHabitacionGuid = linea.ReservaHabitacionGuid,
            ClienteGuid = val.ClienteGuid,
            SucursalGuid = val.SucursalGuid,
            HabitacionGuid = linea.HabitacionGuid,
            Estado = "ACT",
            FechaCheckinUtc = DateTimeOffset.UtcNow,
            CreadoPorUsuario = usuario,
            EsEliminado = false,
            ServicioOrigen = "stay-service"
        };

        var created = await _estadiaData.CrearAsync(model, ct);

        _audit.EmitFireAndForget(
            "stay-service", "estadias.estadia", "INSERT",
            created.EstadiaGuid.ToString(), created.IdEstadia.ToString(),
            Guid.Empty.ToString(), usuario, null,
            null, JsonSerializer.Serialize(created));

        return ToDto(created);
    }

    public async Task<EstadiaDto> CheckOutAsync(Guid estadiaGuid, string usuario, CancellationToken ct = default)
    {
        var m = await _estadiaData.ObtenerPorGuidAsync(estadiaGuid, ct);
        if (m is null) throw new NotFoundException("Estadía", estadiaGuid);
        if (m.Estado != "ACT")
            throw new ConflictException("Estadía", $"Check-out no permitido en estado {m.Estado}.");

        var ok = await _accommodation.UpdateRoomStatusAsync(m.HabitacionGuid, "DIS", estadiaGuid, ct);
        if (!ok)
            throw new ConflictException("Habitación", "No se pudo liberar la habitación en alojamiento (check-out).");

        m.Estado = "FIN";
        m.FechaCheckoutUtc = DateTimeOffset.UtcNow;
        m.ModificadoPorUsuario = usuario;
        m.FechaModificacionUtc = DateTimeOffset.UtcNow;

        var updated = await _estadiaData.ActualizarAsync(m, ct)
                      ?? throw new InvalidOperationException("No se pudo actualizar la estadía.");

        _ = Task.Run(async () =>
        {
            try
            {
                await _finance.GenerateFinalInvoiceAsync(
                    updated.EstadiaGuid,
                    updated.ReservaGuid,
                    updated.ClienteGuid,
                    updated.SucursalGuid,
                    usuario);
            }
            catch
            {
                // La factura final no debe revertir un check-out ya completado.
            }
        }, CancellationToken.None);

        _audit.EmitFireAndForget(
            "stay-service", "estadias.estadia", "UPDATE",
            updated.EstadiaGuid.ToString(), updated.IdEstadia.ToString(),
            Guid.Empty.ToString(), usuario, null,
            JsonSerializer.Serialize(new { estado = "ACT" }),
            JsonSerializer.Serialize(new { estado = "FIN" }));

        return ToDto(updated);
    }

    public async Task<object> ListarAsync(string? estado, Guid? sucursalGuid, int pagina, int limite, CancellationToken ct = default)
    {
        var p = pagina < 1 ? 1 : pagina;
        var l = limite < 1 ? 20 : Math.Min(limite, 200);
        var (items, total) = await _estadiaData.ListarAsync(estado, sucursalGuid, p, l, ct);
        return new
        {
            items = items.Select(ToDto).ToList(),
            total,
            pagina = p,
            limite = l
        };
    }

    public async Task MarcarMantenimientoAsync(Guid estadiaGuid, string usuario, CancellationToken ct = default)
    {
        var m = await _estadiaData.ObtenerPorGuidAsync(estadiaGuid, ct);
        if (m is null) throw new NotFoundException("Estadía", estadiaGuid);

        m.RequiereMantenimiento = true;
        m.ModificadoPorUsuario = usuario;
        m.FechaModificacionUtc = DateTimeOffset.UtcNow;

        _ = await _estadiaData.ActualizarAsync(m, ct)
              ?? throw new InvalidOperationException("No se pudo actualizar la estadía.");

        _audit.EmitFireAndForget(
            "stay-service", "hospedaje.estadia", "UPDATE",
            m.EstadiaGuid.ToString(), m.IdEstadia.ToString(),
            Guid.Empty.ToString(), usuario, null,
            null, JsonSerializer.Serialize(new { requiere_mantenimiento = true }));
    }

    public async Task<EstadiaDto?> ObtenerPorGuidAsync(Guid estadiaGuid, CancellationToken ct = default)
    {
        var m = await _estadiaData.ObtenerPorGuidAsync(estadiaGuid, ct);
        return m is null ? null : ToDto(m);
    }

    private static EstadiaDto ToDto(EstadiaDataModel m) => new()
    {
        EstadiaGuid = m.EstadiaGuid,
        ReservaGuid = m.ReservaGuid,
        ReservaHabitacionGuid = m.ReservaHabitacionGuid,
        ClienteGuid = m.ClienteGuid,
        SucursalGuid = m.SucursalGuid,
        HabitacionGuid = m.HabitacionGuid,
        Estado = m.Estado,
        FechaCheckinUtc = m.FechaCheckinUtc,
        FechaCheckoutUtc = m.FechaCheckoutUtc,
        RequiereMantenimiento = m.RequiereMantenimiento
    };
}
