using HotelLux.Stay.DataAccess.Entities;

namespace HotelLux.Stay.DataAccess.Repositories.Interfaces;

public interface IValoracionRepository
{
    Task<(IReadOnlyList<ValoracionEntity> Items, int Total)> ListarPorSucursalAsync(
        Guid sucursalGuid, int pagina, int pageSize, CancellationToken ct = default);
    Task<IReadOnlyList<ValoracionEntity>> ListarPorClienteAsync(Guid clienteGuid, CancellationToken ct = default);
    Task<ValoracionAgrupada?> ObtenerPromediosPorSucursalAsync(Guid sucursalGuid, CancellationToken ct = default);
    Task<int> ContarPorSucursalAsync(Guid sucursalGuid, CancellationToken ct = default);
    Task AgregarAsync(ValoracionEntity entity, CancellationToken ct = default);

    Task<(IReadOnlyList<ValoracionEntity> Items, int Total)> ListarPaginadoAsync(
        int pagina, int limite, CancellationToken ct = default);

    Task<ValoracionEntity?> ObtenerPorGuidAsync(Guid valoracionGuid, CancellationToken ct = default);
    Task<ValoracionEntity?> ObtenerParaActualizarPorGuidAsync(Guid valoracionGuid, CancellationToken ct = default);
    void Actualizar(ValoracionEntity entity);
}

public sealed class ValoracionAgrupada
{
    public double PromedioGeneral { get; init; }
    public double PromedioLimpieza { get; init; }
    public double PromedioConfort { get; init; }
    public double PromedioUbicacion { get; init; }
    public double PromedioInstalaciones { get; init; }
    public double PromedioPersonal { get; init; }
    public double PromedioCalidadPrecio { get; init; }
    public int Total { get; init; }
}
