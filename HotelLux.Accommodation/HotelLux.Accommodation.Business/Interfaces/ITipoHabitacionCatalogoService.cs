using HotelLux.Accommodation.Business.DTOs.CatalogoServicio;

namespace HotelLux.Accommodation.Business.Interfaces;

public interface ITipoHabitacionCatalogoService
{
    Task<IReadOnlyList<CatalogoServicioDTO>> ListarPorTipoAsync(Guid tipoGuid, CancellationToken ct = default);
    Task AsignarAsync(Guid tipoGuid, Guid catalogoGuid, string usuario, CancellationToken ct = default);
    Task RemoverAsync(Guid tipoGuid, Guid catalogoGuid, CancellationToken ct = default);
    Task RemoverPorIdAsync(Guid tipoGuid, int idTipoHabCatalogo, CancellationToken ct = default);
}
