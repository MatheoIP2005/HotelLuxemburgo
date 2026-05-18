using HotelLux.Accommodation.Business.DTOs.TipoHabitacionImagen;

namespace HotelLux.Accommodation.Business.Interfaces;

public interface ITipoHabitacionImagenService
{
    Task<IReadOnlyList<TipoHabitacionImagenDTO>> ListarPorTipoAsync(Guid tipoGuid, CancellationToken ct = default);
    Task<TipoHabitacionImagenDTO> CrearAsync(Guid tipoGuid, TipoHabitacionImagenCreateDTO dto, CancellationToken ct = default);
    Task EliminarAsync(Guid tipoGuid, int idImagen, CancellationToken ct = default);
}
