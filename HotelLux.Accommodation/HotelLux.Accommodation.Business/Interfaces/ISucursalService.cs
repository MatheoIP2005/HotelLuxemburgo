using HotelLux.Accommodation.Business.DTOs.Sucursal;

namespace HotelLux.Accommodation.Business.Interfaces;

public interface ISucursalService
{
    Task<SucursalDTO> ObtenerPorGuidAsync(Guid guid, CancellationToken ct = default);
    Task<IReadOnlyList<SucursalDTO>> ListarAsync(CancellationToken ct = default);
    Task<SucursalDTO> CrearAsync(SucursalCreateDTO dto, CancellationToken ct = default);
    Task<SucursalDTO> ActualizarAsync(Guid guid, SucursalUpdateDTO dto, CancellationToken ct = default);
    Task InhabilitarAsync(Guid guid, string usuario, CancellationToken ct = default);
    Task EliminarAsync(Guid guid, string usuario, CancellationToken ct = default);
    Task<SucursalDTO> ActualizarPoliticasAsync(Guid guid, SucursalPoliticasPatchDTO dto, CancellationToken ct = default);
}
