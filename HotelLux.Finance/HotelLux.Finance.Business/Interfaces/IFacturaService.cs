using HotelLux.Finance.Business.DTOs;
using HotelLux.Finance.DataManagement.Models;

namespace HotelLux.Finance.Business.Interfaces;

public interface IFacturaService
{
    Task<FacturaDto?> ObtenerPorGuidAsync(Guid facturaGuid, CancellationToken ct = default);
    Task<IReadOnlyList<FacturaDto>> ListarAsync(Guid? clienteGuid, Guid? sucursalGuid, string? estado, CancellationToken ct = default);
    Task<FacturaDataModel> GenerarAsync(FacturaDataModel model, CancellationToken ct = default);
    Task<bool> ActualizarSaldoAsync(Guid facturaGuid, decimal montoPagado, CancellationToken ct = default);
    Task<bool> MarcarPagadaAsync(Guid facturaGuid, CancellationToken ct = default);
    Task<IReadOnlyList<FacturaDetalleDataModel>> ListarDetallesAsync(Guid facturaGuid, CancellationToken ct = default);
    Task<bool> AnularAsync(Guid facturaGuid, string motivo, string usuario, CancellationToken ct = default);
    Task<IReadOnlyList<FacturaDto>> ListarPorReservaGuidAsync(Guid reservaGuid, CancellationToken ct = default);
    Task<FacturaDto> GenerarConLineasAsync(
        string tipoFactura,
        Guid reservaGuid,
        Guid clienteGuid,
        Guid sucursalGuid,
        IReadOnlyList<FacturaLineaGeneracionDto> lineas,
        string usuario,
        CancellationToken ct = default);
}
