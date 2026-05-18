using HotelLux.Reservation.DataManagement.Models;

namespace HotelLux.Reservation.DataManagement.Interfaces;

public interface IClienteDataService
{
    Task<ClienteDataModel?> ObtenerPorGuidAsync(Guid clienteGuid, CancellationToken ct = default);
    Task<ClienteDataModel?> ObtenerPorIdentificacionAsync(string tipoId, string numeroId, CancellationToken ct = default);
    Task<PagedDataResult<ClienteDataModel>> ListarAsync(int pagina, int limite, CancellationToken ct = default);
    Task<ClienteDataModel> CrearAsync(ClienteDataModel model, CancellationToken ct = default);
    Task<ClienteDataModel?> ActualizarAsync(Guid clienteGuid, ClienteDataModel model, CancellationToken ct = default);
    Task<bool> InhabilitarAsync(Guid clienteGuid, string motivo, string usuario, CancellationToken ct = default);
    Task<bool> EliminarLogicoAsync(Guid clienteGuid, string usuario, CancellationToken ct = default);
}
