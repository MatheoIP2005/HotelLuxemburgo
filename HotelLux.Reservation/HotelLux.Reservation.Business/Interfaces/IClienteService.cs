using HotelLux.Reservation.Business.DTOs.Cliente;
using HotelLux.Reservation.Business.DTOs.Common;

namespace HotelLux.Reservation.Business.Interfaces;

public interface IClienteService
{
    Task<ClienteDto?> ObtenerPorGuidAsync(Guid clienteGuid, CancellationToken ct = default);
    Task<PagedResultDTO<ClienteDto>> ListarAsync(int pagina, int limite, CancellationToken ct = default);
    Task<ClienteDto> CrearAsync(ClienteCreateDto dto, CancellationToken ct = default);
    Task<ClienteDto> ActualizarAsync(Guid clienteGuid, ClienteUpdateDto dto, CancellationToken ct = default);
    Task InhabilitarAsync(Guid clienteGuid, string motivo, string usuario, CancellationToken ct = default);
    Task EliminarLogicoAsync(Guid clienteGuid, string usuario, CancellationToken ct = default);
}
