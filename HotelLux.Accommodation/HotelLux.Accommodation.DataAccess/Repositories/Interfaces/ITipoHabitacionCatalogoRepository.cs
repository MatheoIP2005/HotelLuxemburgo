using HotelLux.Accommodation.DataAccess.Entities;

namespace HotelLux.Accommodation.DataAccess.Repositories.Interfaces;

public interface ITipoHabitacionCatalogoRepository
{
    Task<TipoHabitacionCatalogoEntity?> ObtenerAsync(int idTipo, int idCatalogo, CancellationToken ct = default);
    Task<IReadOnlyList<TipoHabitacionCatalogoEntity>> ListarPorTipoAsync(int idTipo, CancellationToken ct = default);
    Task AgregarAsync(TipoHabitacionCatalogoEntity entity, CancellationToken ct = default);
    void Eliminar(TipoHabitacionCatalogoEntity entity);
}
