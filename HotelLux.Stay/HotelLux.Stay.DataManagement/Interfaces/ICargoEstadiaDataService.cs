using HotelLux.Stay.DataManagement.Models;

namespace HotelLux.Stay.DataManagement.Interfaces;

public interface ICargoEstadiaDataService
{
    Task<IReadOnlyList<CargoEstadiaDataModel>> ListarPorEstadiaAsync(Guid estadiaGuid, CancellationToken ct = default);
    Task<CargoEstadiaDataModel> CrearAsync(CargoEstadiaDataModel model, CancellationToken ct = default);
    Task<CargoEstadiaDataModel?> ObtenerPorCargoGuidAsync(Guid cargoGuid, CancellationToken ct = default);
    Task AnularCargoAsync(Guid cargoGuid, string usuario, CancellationToken ct = default);
}
