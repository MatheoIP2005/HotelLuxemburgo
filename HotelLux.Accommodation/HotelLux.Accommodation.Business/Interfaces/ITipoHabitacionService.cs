using HotelLux.Accommodation.Business.DTOs.TipoHabitacion;

namespace HotelLux.Accommodation.Business.Interfaces;

public interface ITipoHabitacionService
{
    Task<TipoHabitacionDTO> ObtenerPorGuidAsync(Guid guid, CancellationToken ct = default);
    Task<IReadOnlyList<TipoHabitacionDTO>> ListarAsync(CancellationToken ct = default);
    Task<TipoHabitacionDTO> CrearAsync(TipoHabitacionCreateDTO dto, CancellationToken ct = default);
    Task<TipoHabitacionDTO> ActualizarAsync(Guid guid, TipoHabitacionUpdateDTO dto, CancellationToken ct = default);
    Task InhabilitarAsync(Guid guid, string usuario, CancellationToken ct = default);
    Task EliminarAsync(Guid guid, string usuario, CancellationToken ct = default);
}
