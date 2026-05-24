using Grpc.Net.Client;
using HotelLux.Protos.Finance;
using HotelLux.Reservation.Business.Interfaces;
using HotelLux.Shared.Grpc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HotelLux.Reservation.API.Clients;

public class FinanceGrpcClient : IFinanceClient
{
    private readonly GrpcChannel _channel;
    private readonly ILogger<FinanceGrpcClient> _logger;

    public FinanceGrpcClient(IConfiguration config, ILogger<FinanceGrpcClient> logger)
    {
        var address = config["FinanceService:GrpcAddress"] ?? config["GrpcClients:FinanceUrl"];
        address = string.IsNullOrWhiteSpace(address)
            ? GrpcChannelFactory.ResolveAddress(config, "FinanceService:GrpcAddress", null, 5105)
            : address.Trim();
        _channel = GrpcChannelFactory.Create(address);
        _logger = logger;
    }

    public async Task<bool> GenerateReservationInvoiceAsync(
        Guid reservaGuid,
        Guid clienteGuid,
        Guid sucursalGuid,
        decimal subtotal,
        decimal valorIva,
        decimal total,
        IEnumerable<(string Descripcion, decimal PrecioUnitario, decimal Cantidad, decimal Subtotal, decimal ValorIva, decimal Total)> lineas,
        string creadoPorUsuario,
        CancellationToken ct = default)
    {
        try
        {
            var client = new FinanceService.FinanceServiceClient(_channel);

            var request = new GenerateReservationInvoiceRequest
            {
                ReservaGuid = reservaGuid.ToString(),
                ClienteGuid = clienteGuid.ToString(),
                SucursalGuid = sucursalGuid.ToString(),
                CreadoPorUsuario = creadoPorUsuario
            };

            request.Items.AddRange(lineas.Select(l => new InvoiceLineItem
            {
                TipoItem = "ALOJAMIENTO",
                ReferenciaTipo = "RESERVA_HABITACION",
                Descripcion = l.Descripcion,
                Cantidad = Math.Max(1, (int)l.Cantidad),
                PrecioUnitario = (double)l.PrecioUnitario,
                Subtotal = (double)l.Subtotal,
                ValorIva = (double)l.ValorIva,
                Descuento = 0,
                Total = (double)l.Total
            }));

            var response = await client.GenerateReservationInvoiceAsync(request, cancellationToken: ct);
            if (!response.Success)
                _logger.LogWarning("Finance GenerateReservationInvoice falló reserva={Reserva}: {Mensaje}",
                    reservaGuid, response.Mensaje);
            return response.Success;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Finance GenerateReservationInvoice error reserva={Reserva}", reservaGuid);
            return false;
        }
    }
}
