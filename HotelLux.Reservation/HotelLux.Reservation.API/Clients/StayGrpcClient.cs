using Grpc.Core;
using Grpc.Net.Client;
using HotelLux.Protos.Stay;
using HotelLux.Reservation.Business.DTOs.Stay;
using HotelLux.Reservation.Business.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HotelLux.Reservation.API.Clients;

public class StayGrpcClient : IStayClient
{
    private readonly GrpcChannel _channel;
    private readonly ILogger<StayGrpcClient> _logger;

    public StayGrpcClient(IConfiguration configuration, ILogger<StayGrpcClient> logger)
    {
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        var address = configuration["StayService:GrpcAddress"] ?? "http://127.0.0.1:5104";
        var handler = new SocketsHttpHandler { EnableMultipleHttp2Connections = true };
        _channel = GrpcChannel.ForAddress(address, new GrpcChannelOptions { HttpHandler = handler });
        _logger = logger;
    }

    public async Task<IReadOnlyList<StayValoracionClienteDto>> GetValoracionesByClienteAsync(
        Guid clienteGuid, CancellationToken ct = default)
    {
        try
        {
            var client = new StayService.StayServiceClient(_channel);
            var resp = await client.GetValoracionesByClienteAsync(new GetValoracionesByClienteRequest
            {
                ClienteGuid = clienteGuid.ToString()
            }, cancellationToken: ct);

            var list = new List<StayValoracionClienteDto>();
            foreach (var v in resp.Valoraciones)
            {
                if (!Guid.TryParse(v.ValoracionGuid, out var vg) || !Guid.TryParse(v.ClienteGuid, out var cg))
                    continue;
                list.Add(new StayValoracionClienteDto
                {
                    ValoracionGuid = vg,
                    ClienteGuid = cg,
                    PuntuacionGeneral = v.PuntuacionGeneral,
                    ComentarioPositivo = v.ComentarioPositivo,
                    ComentarioNegativo = v.ComentarioNegativo,
                    TipoViaje = v.TipoViaje,
                    FechaPublicacion = v.FechaPublicacion,
                    RespuestaHotel = v.RespuestaHotel,
                    NombreVisibleCliente = v.NombreVisibleCliente
                });
            }

            return list;
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(ex, "Stay gRPC GetValoracionesByCliente falló (cliente={ClienteGuid})", clienteGuid);
            return Array.Empty<StayValoracionClienteDto>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Stay gRPC GetValoracionesByCliente error (cliente={ClienteGuid})", clienteGuid);
            return Array.Empty<StayValoracionClienteDto>();
        }
    }
}
