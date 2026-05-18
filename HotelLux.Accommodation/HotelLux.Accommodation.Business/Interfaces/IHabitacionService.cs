using HotelLux.Accommodation.Business.DTOs.Habitacion;

namespace HotelLux.Accommodation.Business.Interfaces;

public interface IHabitacionService
{
    Task<HabitacionDTO> ObtenerPorGuidAsync(Guid guid, CancellationToken ct = default);
    Task<IReadOnlyList<HabitacionDTO>> ListarAsync(CancellationToken ct = default);
    Task<IReadOnlyList<HabitacionDTO>> ListarPorSucursalAsync(Guid sucursalGuid, CancellationToken ct = default);
    Task<IReadOnlyList<HabitacionDTO>> ListarDisponiblesAsync(Guid sucursalGuid, DateOnly fechaEntrada, DateOnly fechaSalida, CancellationToken ct = default);
    Task<HabitacionDTO> CrearAsync(HabitacionCreateDTO dto, CancellationToken ct = default);
    Task<HabitacionDTO> ActualizarAsync(Guid guid, HabitacionUpdateDTO dto, CancellationToken ct = default);
    Task CambiarEstadoAsync(Guid guid, string nuevoEstado, string usuario, CancellationToken ct = default);
    Task InhabilitarAsync(Guid guid, string usuario, CancellationToken ct = default);
    Task EliminarAsync(Guid guid, string usuario, CancellationToken ct = default);
}
