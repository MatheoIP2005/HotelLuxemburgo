using System.Globalization;
using Grpc.Net.Client;
using HotelLux.Protos.Reservation;
using HotelLux.Stay.Business.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HotelLux.Stay.API.Clients;

public class ReservationStayGrpcClient : IReservationStayClient
{
    private readonly GrpcChannel _channel;
    private readonly ILogger<ReservationStayGrpcClient> _logger;

    public ReservationStayGrpcClient(IConfiguration config, ILogger<ReservationStayGrpcClient> logger)
    {
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        var address = config["ReservationService:GrpcAddress"] ?? "http://localhost:5103";
        var handler = new SocketsHttpHandler { EnableMultipleHttp2Connections = true };
        _channel = GrpcChannel.ForAddress(address, new GrpcChannelOptions { HttpHandler = handler });
        _logger = logger;
    }

    public async Task<ValidacionCheckinResult> ValidarReservaParaCheckinAsync(Guid reservaGuid, CancellationToken ct = default)
    {
        try
        {
            var client = new ReservationService.ReservationServiceClient(_channel);
            var r = await client.ValidateReservationForCheckinAsync(
                new ValidateReservationForCheckinRequest { ReservaGuid = reservaGuid.ToString() },
                cancellationToken: ct);

            if (!r.Valid)
                return new ValidacionCheckinResult { Valid = false, Mensaje = r.Mensaje };

            if (!Guid.TryParse(r.ClienteGuid, out var cliente) || !Guid.TryParse(r.SucursalGuid, out var sucursal))
            {
                _logger.LogWarning("Validación reserva: GUIDs cliente/sucursal inválidos");
                return new ValidacionCheckinResult { Valid = false, Mensaje = "Respuesta de reservas inválida (GUIDs)." };
            }

            var lineas = new List<ReservaHabitacionValidada>();
            foreach (var h in r.Habitaciones)
            {
                if (!Guid.TryParse(h.ReservaHabitacionGuid, out var rh) ||
                    !Guid.TryParse(h.HabitacionGuid, out var hab))
                    continue;
                if (!DateOnly.TryParse(h.FechaInicio, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fi) ||
                    !DateOnly.TryParse(h.FechaFin, CultureInfo.InvariantCulture, DateTimeStyles.None, out var ff))
                    continue;
                lineas.Add(new ReservaHabitacionValidada
                {
                    ReservaHabitacionGuid = rh,
                    HabitacionGuid = hab,
                    FechaInicio = fi,
                    FechaFin = ff
                });
            }

            return new ValidacionCheckinResult
            {
                Valid = true,
                Mensaje = string.Empty,
                ClienteGuid = cliente,
                SucursalGuid = sucursal,
                Lineas = lineas
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ValidateReservationForCheckin gRPC error reserva={Res}", reservaGuid);
            return new ValidacionCheckinResult { Valid = false, Mensaje = "Error al comunicar con el servicio de reservas." };
        }
    }
}
