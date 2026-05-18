using HotelLux.Finance.DataManagement.Models;

namespace HotelLux.Finance.DataManagement.Interfaces;

public interface IFacturaDataService
{
    Task<FacturaDataModel?> ObtenerPorGuidAsync(Guid facturaGuid, CancellationToken ct = default);
    Task<IReadOnlyList<FacturaDataModel>> ListarAsync(Guid? clienteGuid, Guid? sucursalGuid, string? estado, CancellationToken ct = default);
    Task<IReadOnlyList<FacturaDataModel>> ListarPorReservaGuidAsync(Guid reservaGuid, CancellationToken ct = default);
    Task<FacturaDataModel> CrearAsync(FacturaDataModel model, CancellationToken ct = default);
    Task<bool> ActualizarSaldoAsync(Guid facturaGuid, decimal montoPagado, CancellationToken ct = default);
    Task<bool> MarcarPagadaAsync(Guid facturaGuid, CancellationToken ct = default);
    Task<bool> AnularAsync(Guid facturaGuid, string motivo, string usuario, CancellationToken ct = default);
}
