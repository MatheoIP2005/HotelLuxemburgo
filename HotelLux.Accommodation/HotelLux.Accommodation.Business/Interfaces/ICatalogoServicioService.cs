using HotelLux.Accommodation.Business.DTOs.CatalogoServicio;

namespace HotelLux.Accommodation.Business.Interfaces;

public interface ICatalogoServicioService
{
    Task<CatalogoServicioDTO> ObtenerPorGuidAsync(Guid guid, CancellationToken ct = default);
    Task<IReadOnlyList<CatalogoServicioDTO>> ListarAsync(CancellationToken ct = default);
    Task<CatalogoServicioDTO> CrearAsync(CatalogoServicioCreateDTO dto, CancellationToken ct = default);
    Task<CatalogoServicioDTO> ActualizarAsync(Guid guid, CatalogoServicioUpdateDTO dto, CancellationToken ct = default);
    Task DesactivarAsync(Guid guid, string usuario, CancellationToken ct = default);
    Task EliminarAsync(Guid guid, string usuario, CancellationToken ct = default);
}
