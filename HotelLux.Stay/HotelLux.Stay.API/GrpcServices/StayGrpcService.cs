using System.Globalization;
using Grpc.Core;
using HotelLux.Protos.Stay;
using HotelLux.Stay.DataManagement.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace HotelLux.Stay.API.GrpcServices;

[AllowAnonymous]
public class StayGrpcService : StayService.StayServiceBase
{
    private readonly IEstadiaDataService _estadiaData;
    private readonly IValoracionDataService _valoracionData;

    public StayGrpcService(
        IEstadiaDataService estadiaData,
        IValoracionDataService valoracionData)
    {
        _estadiaData = estadiaData;
        _valoracionData = valoracionData;
    }

    public override async Task<ValidateStayCompletedResponse> ValidateStayCompleted(
        ValidateStayCompletedRequest request, ServerCallContext context)
    {
        var resp = new ValidateStayCompletedResponse();
        if (!Guid.TryParse(request.EstadiaGuid, out var estadiaGuid))
            return resp;

        var e = await _estadiaData.ObtenerPorGuidAsync(estadiaGuid, context.CancellationToken);
        if (e is null)
            return resp;

        resp.Completed = e.Estado == "FIN";
        resp.ReservaGuid = e.ReservaGuid.ToString();
        resp.ClienteGuid = e.ClienteGuid.ToString();
        return resp;
    }

    public override async Task<GetStayStatusResponse> GetStayStatus(
        GetStayStatusRequest request, ServerCallContext context)
    {
        var resp = new GetStayStatusResponse();
        if (!Guid.TryParse(request.EstadiaGuid, out var estadiaGuid))
            return resp;

        var e = await _estadiaData.ObtenerPorGuidAsync(estadiaGuid, context.CancellationToken);
        if (e is null)
            return resp;

        resp.Encontrada = true;
        resp.Estado = e.Estado;
        resp.ClienteGuid = e.ClienteGuid.ToString();
        resp.HabitacionGuid = e.HabitacionGuid.ToString();
        resp.CheckinUtc = e.FechaCheckinUtc?.UtcDateTime.ToString("o", CultureInfo.InvariantCulture) ?? string.Empty;
        resp.CheckoutUtc = e.FechaCheckoutUtc?.UtcDateTime.ToString("o", CultureInfo.InvariantCulture) ?? string.Empty;
        return resp;
    }

    public override async Task<GetReviewsBySucursalResponse> GetReviewsBySucursal(
        GetReviewsBySucursalRequest request, ServerCallContext context)
    {
        var resp = new GetReviewsBySucursalResponse();
        if (!Guid.TryParse(request.SucursalGuid, out var sucursalGuid))
            return resp;

        var pagina = request.Page < 1 ? 1 : request.Page;
        var size = request.PageSize < 1 ? 20 : Math.Min(request.PageSize, 100);

        var (items, total) = await _valoracionData.ListarPorSucursalAsync(sucursalGuid, pagina, size, context.CancellationToken);
        foreach (var v in items)
        {
            resp.Reviews.Add(new ReviewItem
            {
                ValoracionGuid = v.ValoracionGuid.ToString(),
                ClienteGuid = v.ClienteGuid.ToString(),
                PuntuacionGeneral = (double)v.PuntuacionGeneral,
                ComentarioPositivo = v.ComentarioPositivo,
                ComentarioNegativo = v.ComentarioNegativo,
                TipoViaje = v.TipoViaje,
                FechaPublicacion = v.FechaPublicacionUtc.UtcDateTime.ToString("o", CultureInfo.InvariantCulture),
                RespuestaHotel = v.RespuestaHotel ?? string.Empty,
                NombreVisibleCliente = v.NombreVisibleCliente ?? string.Empty
            });
        }

        resp.TotalItems = total;
        resp.TotalPaginas = size > 0 ? (int)Math.Ceiling((double)total / size) : 0;
        return resp;
    }

    public override async Task<GetRatingSummaryResponse> GetRatingSummary(
        GetRatingSummaryRequest request, ServerCallContext context)
    {
        var resp = new GetRatingSummaryResponse();
        if (!Guid.TryParse(request.SucursalGuid, out var sucursalGuid))
            return resp;

        var s = await _valoracionData.ObtenerResumenPorSucursalAsync(sucursalGuid, context.CancellationToken);
        resp.TieneResenas = s.TieneResenas;
        resp.PromedioGeneral = s.PromedioGeneral;
        resp.PromedioLimpieza = s.PromedioLimpieza;
        resp.PromedioConfort = s.PromedioConfort;
        resp.PromedioUbicacion = s.PromedioUbicacion;
        resp.PromedioInstalaciones = s.PromedioInstalaciones;
        resp.PromedioPersonal = s.PromedioPersonal;
        resp.PromedioCalidadPrecio = s.PromedioCalidadPrecio;
        resp.TotalResenas = s.TotalResenas;
        return resp;
    }

    public override async Task<GetValoracionesByClienteResponse> GetValoracionesByCliente(
        GetValoracionesByClienteRequest request, ServerCallContext context)
    {
        var resp = new GetValoracionesByClienteResponse();
        if (!Guid.TryParse(request.ClienteGuid, out var clienteGuid))
            return resp;

        var list = await _valoracionData.ListarPorClienteAsync(clienteGuid, context.CancellationToken);
        foreach (var v in list)
        {
            resp.Valoraciones.Add(new ReviewItem
            {
                ValoracionGuid = v.ValoracionGuid.ToString(),
                ClienteGuid = v.ClienteGuid.ToString(),
                PuntuacionGeneral = (double)v.PuntuacionGeneral,
                ComentarioPositivo = v.ComentarioPositivo,
                ComentarioNegativo = v.ComentarioNegativo,
                TipoViaje = v.TipoViaje,
                FechaPublicacion = v.FechaPublicacionUtc.UtcDateTime.ToString("o", CultureInfo.InvariantCulture),
                RespuestaHotel = v.RespuestaHotel ?? string.Empty,
                NombreVisibleCliente = v.NombreVisibleCliente ?? string.Empty
            });
        }

        return resp;
    }
}
