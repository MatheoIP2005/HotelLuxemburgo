using HotelLux.Accommodation.Business.DTOs.Tarifa;

namespace HotelLux.Accommodation.Business.Interfaces;

public interface ITarifaService
{
    Task<TarifaDTO> ObtenerPorGuidAsync(Guid guid, CancellationToken ct = default);
    Task<IReadOnlyList<TarifaDTO>> ListarAsync(CancellationToken ct = default);
    Task<TarifaDTO> CrearAsync(TarifaCreateDTO dto, CancellationToken ct = default);
    Task<TarifaDTO> ActualizarAsync(Guid guid, TarifaUpdateDTO dto, CancellationToken ct = default);
    Task DesactivarAsync(Guid guid, string usuario, CancellationToken ct = default);
    Task EliminarAsync(Guid guid, string usuario, CancellationToken ct = default);
}
