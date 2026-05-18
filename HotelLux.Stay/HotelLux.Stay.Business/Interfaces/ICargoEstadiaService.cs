using HotelLux.Stay.Business.DTOs;

namespace HotelLux.Stay.Business.Interfaces;

public interface ICargoEstadiaService
{
    Task<CargoEstadiaDto> CrearAsync(Guid estadiaGuid, CargoEstadiaCreateDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<CargoEstadiaDto>> ListarPorEstadiaAsync(Guid estadiaGuid, CancellationToken ct = default);
    Task<CargoEstadiaDto> ObtenerPorGuidAsync(Guid cargoGuid, CancellationToken ct = default);
    Task AnularAsync(Guid cargoGuid, string usuario, CancellationToken ct = default);
}
