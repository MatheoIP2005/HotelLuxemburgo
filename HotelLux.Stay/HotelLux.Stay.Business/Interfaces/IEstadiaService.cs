using HotelLux.Stay.Business.DTOs;

namespace HotelLux.Stay.Business.Interfaces;

public interface IEstadiaService
{
    Task<EstadiaDto> CheckInAsync(CheckInDto dto, CancellationToken ct = default);
    Task<EstadiaDto> CheckOutAsync(Guid estadiaGuid, string usuario, CancellationToken ct = default);
    Task<EstadiaDto?> ObtenerPorGuidAsync(Guid estadiaGuid, CancellationToken ct = default);
    Task<object> ListarAsync(string? estado, Guid? sucursalGuid, int pagina, int limite, CancellationToken ct = default);
    Task MarcarMantenimientoAsync(Guid estadiaGuid, string usuario, CancellationToken ct = default);
}
