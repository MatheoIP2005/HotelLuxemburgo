using System.Globalization;
using Grpc.Core;
using HotelLux.Protos.Reservation;
using HotelLux.Reservation.DataManagement.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace HotelLux.Reservation.API.GrpcServices;

/// <summary>
/// gRPC interno consumido por Stay. Sin JWT: confiar en red / mTLS en despliegue.
/// </summary>
[AllowAnonymous]
public class ReservationGrpcService : ReservationService.ReservationServiceBase
{
    private readonly IReservaDataService _reservaData;
    private readonly ILogger<ReservationGrpcService> _logger;

    public ReservationGrpcService(
        IReservaDataService reservaData,
        ILogger<ReservationGrpcService> logger)
    {
        _reservaData = reservaData;
        _logger = logger;
    }

    public override async Task<GetReservationResponse> GetReservation(
        GetReservationRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.ReservaGuid, out var reservaGuid))
        {
            _logger.LogWarning("GetReservation: ReservaGuid inválido '{Raw}'", request.ReservaGuid);
            return new GetReservationResponse { Encontrada = false };
        }

        var m = await _reservaData.ObtenerPorGuidAsync(reservaGuid, context.CancellationToken);
        if (m is null)
            return new GetReservationResponse { Encontrada = false };

        var resp = new GetReservationResponse
        {
            Encontrada = true,
            ReservaGuid = m.ReservaGuid.ToString(),
            CodigoReserva = m.CodigoReserva,
            ClienteGuid = m.ClienteGuid.ToString(),
            SucursalGuid = m.SucursalGuid.ToString(),
            Estado = m.EstadoReserva,
            TotalReserva = (double)m.TotalReserva,
            SaldoPendiente = (double)m.SaldoPendiente,
            FechaInicio = ToIsoDate(m.FechaInicio),
            FechaFin = ToIsoDate(m.FechaFin)
        };

        foreach (var h in m.Habitaciones.OrderBy(x => x.FechaInicio))
            resp.HabitacionGuids.Add(h.HabitacionGuid.ToString());

        _logger.LogDebug("GetReservation: {Guid} estado={Estado}", reservaGuid, m.EstadoReserva);
        return resp;
    }

    public override async Task<ValidateReservationForCheckinResponse> ValidateReservationForCheckin(
        ValidateReservationForCheckinRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.ReservaGuid, out var reservaGuid))
        {
            _logger.LogWarning("ValidateReservationForCheckin: ReservaGuid inválido '{Raw}'", request.ReservaGuid);
            return Invalid("GUID de reserva inválido.");
        }

        var m = await _reservaData.ObtenerPorGuidAsync(reservaGuid, context.CancellationToken);
        if (m is null)
            return Invalid("Reserva no encontrada.");

        if (m.EstadoReserva != "CON")
            return Invalid($"La reserva no está confirmada (estado: {m.EstadoReserva}).");

        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        if (hoy > m.FechaFin)
        {
            return Invalid(
                $"Check-in no permitido: la reserva ya finalizó el {ToIsoDate(m.FechaFin)}.");
        }

        if (m.Habitaciones.Count == 0)
            return Invalid("La reserva no tiene líneas de habitación.");

        var lineasConHabitacion = m.Habitaciones
            .Where(h => h.HabitacionGuid != Guid.Empty && h.EstadoDetalle == "CON")
            .ToList();

        if (lineasConHabitacion.Count == 0)
        {
            return Invalid(
                "No hay líneas de habitación confirmadas (CON) para hacer check-in. " +
                "Confirme la reserva antes de registrar la estadía.");
        }

        var resp = new ValidateReservationForCheckinResponse
        {
            Valid = true,
            Mensaje = string.Empty,
            ClienteGuid = m.ClienteGuid.ToString(),
            SucursalGuid = m.SucursalGuid.ToString()
        };

        foreach (var h in lineasConHabitacion.OrderBy(x => x.FechaInicio))
        {
            resp.Habitaciones.Add(new ReservaHabitacionLite
            {
                ReservaHabitacionGuid = h.ReservaHabitacionGuid.ToString(),
                HabitacionGuid = h.HabitacionGuid.ToString(),
                FechaInicio = ToIsoDate(h.FechaInicio),
                FechaFin = ToIsoDate(h.FechaFin)
            });
        }

        _logger.LogInformation(
            "ValidateReservationForCheckin OK reserva={Guid} líneas={Count}", reservaGuid, resp.Habitaciones.Count);
        return resp;

        static ValidateReservationForCheckinResponse Invalid(string mensaje) =>
            new() { Valid = false, Mensaje = mensaje };
    }

    private static string ToIsoDate(DateOnly date) =>
        date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
