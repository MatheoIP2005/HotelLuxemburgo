using HotelLux.Stay.DataAccess.Repositories.Interfaces;
using HotelLux.Stay.DataManagement.Models;

namespace HotelLux.Stay.DataManagement.Interfaces;

public interface IValoracionDataService
{
    Task<(IReadOnlyList<ValoracionDataModel> Items, int Total)> ListarPorSucursalAsync(
        Guid sucursalGuid, int pagina, int pageSize, CancellationToken ct = default);
    Task<IReadOnlyList<ValoracionDataModel>> ListarPorClienteAsync(Guid clienteGuid, CancellationToken ct = default);
    Task<RatingSummaryDataModel> ObtenerResumenPorSucursalAsync(Guid sucursalGuid, CancellationToken ct = default);
    Task<ValoracionDataModel> CrearAsync(ValoracionDataModel model, CancellationToken ct = default);

    Task<(IReadOnlyList<ValoracionDataModel> Items, int Total)> ListarPaginadoAsync(
        int pagina, int limite, CancellationToken ct = default);

    Task<ValoracionDataModel?> ObtenerPorGuidAsync(Guid valoracionGuid, CancellationToken ct = default);
    Task ActualizarRespuestaAsync(Guid valoracionGuid, string respuesta, string usuario, CancellationToken ct = default);
    Task MarcarOcultaModeracionAsync(Guid valoracionGuid, string usuario, CancellationToken ct = default);
}
