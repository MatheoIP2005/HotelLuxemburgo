using Grpc.Core;
using Grpc.Net.Client;
using HotelLux.Protos.Stay;
using HotelLux.Shared.Grpc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HotelLux.Accommodation.API.Services;

public sealed class StayPublicGrpcClient : IStayPublicClient
{
    private readonly GrpcChannel _channel;
    private readonly ILogger<StayPublicGrpcClient> _logger;

    public StayPublicGrpcClient(IConfiguration configuration, ILogger<StayPublicGrpcClient> logger)
    {
        var address = GrpcChannelFactory.ResolveAddress(configuration, "StayService:GrpcAddress", null, 5104);
        _channel = GrpcChannelFactory.Create(address);
        _logger = logger;
    }

    public async Task<StayReviewsResult?> GetReviewsBySucursalAsync(
        Guid sucursalGuid, int page, int pageSize, CancellationToken ct)
    {
        try
        {
            var client = new StayService.StayServiceClient(_channel);

            var resp = await client.GetReviewsBySucursalAsync(new GetReviewsBySucursalRequest
            {
                SucursalGuid = sucursalGuid.ToString(),
                Page = page,
                PageSize = pageSize
            }, cancellationToken: ct);

            var items = resp.Reviews.Select(r => new StayReviewDto
            {
                ValoracionGuid = Guid.TryParse(r.ValoracionGuid, out var vg) ? vg : Guid.Empty,
                ClienteGuid = Guid.TryParse(r.ClienteGuid, out var cg) ? cg : Guid.Empty,
                PuntuacionGeneral = (decimal)r.PuntuacionGeneral,
                ComentarioPositivo = r.ComentarioPositivo ?? "",
                ComentarioNegativo = r.ComentarioNegativo ?? "",
                TipoViaje = r.TipoViaje ?? "",
                FechaPublicacion = r.FechaPublicacion ?? "",
                RespuestaHotel = r.RespuestaHotel ?? "",
                NombreVisibleCliente = string.IsNullOrEmpty(r.NombreVisibleCliente) ? null : r.NombreVisibleCliente
            }).ToList();

            return new StayReviewsResult
            {
                Items = items,
                TotalItems = resp.TotalItems,
                TotalPages = resp.TotalPaginas
            };
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(ex, "Stay gRPC GetReviewsBySucursal no disponible (sucursal={SucursalGuid})", sucursalGuid);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar reseñas en Stay (sucursal={SucursalGuid})", sucursalGuid);
            return null;
        }
    }

    public async Task<StayRatingSummary?> GetRatingSummaryAsync(Guid sucursalGuid, CancellationToken ct)
    {
        try
        {
            var client = new StayService.StayServiceClient(_channel);

            var resp = await client.GetRatingSummaryAsync(new GetRatingSummaryRequest
            {
                SucursalGuid = sucursalGuid.ToString()
            }, cancellationToken: ct);

            return new StayRatingSummary
            {
                TieneResenas = resp.TieneResenas,
                PromedioGeneral = resp.PromedioGeneral,
                PromedioLimpieza = resp.PromedioLimpieza,
                PromedioConfort = resp.PromedioConfort,
                PromedioUbicacion = resp.PromedioUbicacion,
                PromedioInstalaciones = resp.PromedioInstalaciones,
                PromedioPersonal = resp.PromedioPersonal,
                PromedioCalidadPrecio = resp.PromedioCalidadPrecio,
                TotalResenas = resp.TotalResenas
            };
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(ex, "Stay gRPC GetRatingSummary no disponible (sucursal={SucursalGuid})", sucursalGuid);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar resumen de rating en Stay (sucursal={SucursalGuid})", sucursalGuid);
            return null;
        }
    }
}
