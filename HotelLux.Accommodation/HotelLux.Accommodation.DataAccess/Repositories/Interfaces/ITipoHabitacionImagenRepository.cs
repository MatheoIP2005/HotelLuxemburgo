using HotelLux.Accommodation.DataAccess.Entities;

namespace HotelLux.Accommodation.DataAccess.Repositories.Interfaces;

public interface ITipoHabitacionImagenRepository
{
    Task<IReadOnlyList<TipoHabitacionImagenEntity>> ListarPorTipoAsync(int idTipoHabitacion, CancellationToken ct = default);
    Task AgregarAsync(TipoHabitacionImagenEntity entity, CancellationToken ct = default);
    Task<TipoHabitacionImagenEntity?> ObtenerPorIdAsync(int idImagen, CancellationToken ct = default);
    void Eliminar(TipoHabitacionImagenEntity entity);
}
