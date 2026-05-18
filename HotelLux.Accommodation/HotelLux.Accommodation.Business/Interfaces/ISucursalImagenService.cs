using HotelLux.Accommodation.Business.DTOs.SucursalImagen;

namespace HotelLux.Accommodation.Business.Interfaces;

public interface ISucursalImagenService
{
    Task<IReadOnlyList<SucursalImagenDTO>> ListarPorSucursalAsync(Guid sucursalGuid, CancellationToken ct = default);
    Task<SucursalImagenDTO> CrearAsync(Guid sucursalGuid, SucursalImagenCreateDTO dto, CancellationToken ct = default);
    Task EliminarAsync(Guid sucursalGuid, Guid imagenGuid, CancellationToken ct = default);
    Task EliminarPorIdSucursalImagenAsync(Guid sucursalGuid, int idSucursalImagen, CancellationToken ct = default);
}
