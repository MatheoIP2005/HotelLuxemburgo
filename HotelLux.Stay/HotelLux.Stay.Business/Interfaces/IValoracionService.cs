using HotelLux.Stay.Business.DTOs;

namespace HotelLux.Stay.Business.Interfaces;

public interface IValoracionService
{
    Task<ValoracionDto> CrearAsync(ValoracionCreateDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<ValoracionDto>> ListarPorEstadiaAsync(Guid estadiaGuid, CancellationToken ct = default);
    Task<(IReadOnlyList<ValoracionDto> Items, int Total)> ListarPaginadoAsync(int pagina, int limite, CancellationToken ct = default);
    Task<ValoracionDto?> ObtenerPorGuidAsync(Guid valoracionGuid, CancellationToken ct = default);
    Task ResponderAsync(Guid valoracionGuid, string respuesta, string usuario, CancellationToken ct = default);
    Task ModerarOcultarAsync(Guid valoracionGuid, string usuario, CancellationToken ct = default);
    Task EliminarAsync(Guid valoracionGuid, string usuario, CancellationToken ct = default);
}
