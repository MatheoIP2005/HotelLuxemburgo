using HotelLux.Stay.DataManagement.Interfaces;
using HotelLux.Stay.DataManagement.Mappers;
using HotelLux.Stay.DataManagement.Models;

namespace HotelLux.Stay.DataManagement.Services;

public class ValoracionDataService : IValoracionDataService
{
    private readonly IUnitOfWork _uow;
    public ValoracionDataService(IUnitOfWork uow) => _uow = uow;

    public async Task<(IReadOnlyList<ValoracionDataModel> Items, int Total)> ListarPorSucursalAsync(
        Guid sucursalGuid, int pagina, int pageSize, CancellationToken ct = default)
    {
        var (items, total) = await _uow.ValoracionRepository.ListarPorSucursalAsync(sucursalGuid, pagina, pageSize, ct);
        return (items.Select(ValoracionDataMapper.ToModel).ToList(), total);
    }

    public async Task<IReadOnlyList<ValoracionDataModel>> ListarPorClienteAsync(Guid clienteGuid, CancellationToken ct = default)
    {
        var list = await _uow.ValoracionRepository.ListarPorClienteAsync(clienteGuid, ct);
        return list.Select(ValoracionDataMapper.ToModel).ToList();
    }

    public async Task<RatingSummaryDataModel> ObtenerResumenPorSucursalAsync(Guid sucursalGuid, CancellationToken ct = default)
    {
        var agg = await _uow.ValoracionRepository.ObtenerPromediosPorSucursalAsync(sucursalGuid, ct);
        if (agg is null || agg.Total == 0)
        {
            return new RatingSummaryDataModel { TieneResenas = false };
        }

        return new RatingSummaryDataModel
        {
            TieneResenas = true,
            PromedioGeneral = agg.PromedioGeneral,
            PromedioLimpieza = agg.PromedioLimpieza,
            PromedioConfort = agg.PromedioConfort,
            PromedioUbicacion = agg.PromedioUbicacion,
            PromedioInstalaciones = agg.PromedioInstalaciones,
            PromedioPersonal = agg.PromedioPersonal,
            PromedioCalidadPrecio = agg.PromedioCalidadPrecio,
            TotalResenas = agg.Total
        };
    }

    public async Task<ValoracionDataModel> CrearAsync(ValoracionDataModel model, CancellationToken ct = default)
    {
        var e = ValoracionDataMapper.ToEntity(model);
        if (e.ValoracionGuid == Guid.Empty)
            e.ValoracionGuid = Guid.NewGuid();
        e.FechaRegistroUtc = DateTimeOffset.UtcNow;
        await _uow.ValoracionRepository.AgregarAsync(e, ct);
        await _uow.SaveChangesAsync(ct);
        return ValoracionDataMapper.ToModel(e);
    }

    public async Task<(IReadOnlyList<ValoracionDataModel> Items, int Total)> ListarPaginadoAsync(
        int pagina, int limite, CancellationToken ct = default)
    {
        var p = pagina < 1 ? 1 : pagina;
        var l = limite < 1 ? 20 : Math.Min(limite, 200);
        var (items, total) = await _uow.ValoracionRepository.ListarPaginadoAsync(p, l, ct);
        return (items.Select(ValoracionDataMapper.ToModel).ToList(), total);
    }

    public async Task<ValoracionDataModel?> ObtenerPorGuidAsync(Guid valoracionGuid, CancellationToken ct = default)
    {
        var e = await _uow.ValoracionRepository.ObtenerPorGuidAsync(valoracionGuid, ct);
        return e is null ? null : ValoracionDataMapper.ToModel(e);
    }

    public async Task ActualizarRespuestaAsync(
        Guid valoracionGuid, string respuesta, string usuario, CancellationToken ct = default)
    {
        var e = await _uow.ValoracionRepository.ObtenerParaActualizarPorGuidAsync(valoracionGuid, ct)
            ?? throw new InvalidOperationException($"Valoración '{valoracionGuid}' no encontrada.");
        e.RespuestaHotel = respuesta.Trim();
        _uow.ValoracionRepository.Actualizar(e);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task MarcarOcultaModeracionAsync(Guid valoracionGuid, string usuario, CancellationToken ct = default)
    {
        var e = await _uow.ValoracionRepository.ObtenerParaActualizarPorGuidAsync(valoracionGuid, ct)
            ?? throw new InvalidOperationException($"Valoración '{valoracionGuid}' no encontrada.");
        e.EsEliminado = true;
        _uow.ValoracionRepository.Actualizar(e);
        await _uow.SaveChangesAsync(ct);
    }
}
