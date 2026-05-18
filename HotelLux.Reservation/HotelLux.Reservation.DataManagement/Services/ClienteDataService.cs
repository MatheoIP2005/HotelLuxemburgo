using HotelLux.Reservation.DataAccess.Entities;
using HotelLux.Reservation.DataManagement.Interfaces;
using HotelLux.Reservation.DataManagement.Mappers;
using HotelLux.Reservation.DataManagement.Models;

namespace HotelLux.Reservation.DataManagement.Services;

public class ClienteDataService : IClienteDataService
{
    private readonly IUnitOfWork _uow;
    public ClienteDataService(IUnitOfWork uow) => _uow = uow;

    public async Task<ClienteDataModel?> ObtenerPorGuidAsync(Guid clienteGuid, CancellationToken ct = default)
    {
        var e = await _uow.ClienteRepository.ObtenerPorGuidAsync(clienteGuid, ct);
        return e is null ? null : ClienteDataMapper.ToDataModel(e);
    }

    public async Task<ClienteDataModel?> ObtenerPorIdentificacionAsync(string tipoId, string numeroId, CancellationToken ct = default)
    {
        var e = await _uow.ClienteRepository.ObtenerPorIdentificacionAsync(tipoId, numeroId, ct);
        return e is null ? null : ClienteDataMapper.ToDataModel(e);
    }

    public async Task<PagedDataResult<ClienteDataModel>> ListarAsync(int pagina, int limite, CancellationToken ct = default)
    {
        var (items, total) = await _uow.ClienteRepository.ListarAsync(pagina, limite, ct);
        return new PagedDataResult<ClienteDataModel>
        {
            Items = items.Select(ClienteDataMapper.ToDataModel).ToList(),
            Total = total,
            Pagina = pagina,
            Limite = limite
        };
    }

    public async Task<ClienteDataModel> CrearAsync(ClienteDataModel model, CancellationToken ct = default)
    {
        if (await _uow.ClienteRepository.ExisteCorreoAsync(model.Correo, null, ct))
            throw new InvalidOperationException($"Ya existe un cliente con el correo '{model.Correo}'.");

        var dup = await _uow.ClienteRepository.ObtenerPorIdentificacionAsync(
            model.TipoIdentificacion, model.NumeroIdentificacion, ct);
        if (dup is not null)
            throw new InvalidOperationException("Ya existe un cliente con el mismo tipo y número de identificación.");

        var e = new ClienteEntity
        {
            ClienteGuid = Guid.NewGuid(),
            TipoIdentificacion = model.TipoIdentificacion.Trim(),
            NumeroIdentificacion = model.NumeroIdentificacion.Trim(),
            Nombres = model.Nombres.Trim(),
            Apellidos = string.IsNullOrWhiteSpace(model.Apellidos) ? null : model.Apellidos.Trim(),
            RazonSocial = string.IsNullOrWhiteSpace(model.RazonSocial) ? null : model.RazonSocial.Trim(),
            Correo = model.Correo.Trim(),
            Telefono = model.Telefono.Trim(),
            Direccion = model.Direccion.Trim(),
            Estado = "ACT",
            EsEliminado = false,
            CreadoPorUsuario = model.CreadoPorUsuario,
            FechaRegistroUtc = DateTimeOffset.UtcNow,
            ServicioOrigen = "reservation-service"
        };

        await _uow.ClienteRepository.AgregarAsync(e, ct);
        await _uow.SaveChangesAsync(ct);

        var reloaded = await _uow.ClienteRepository.ObtenerPorGuidAsync(e.ClienteGuid, ct) ?? e;
        return ClienteDataMapper.ToDataModel(reloaded);
    }

    public async Task<ClienteDataModel?> ActualizarAsync(Guid clienteGuid, ClienteDataModel model, CancellationToken ct = default)
    {
        var e = await _uow.ClienteRepository.ObtenerParaActualizarAsync(clienteGuid, ct);
        if (e is null) return null;

        if (await _uow.ClienteRepository.ExisteCorreoAsync(model.Correo, clienteGuid, ct))
            throw new InvalidOperationException($"Ya existe otro cliente con el correo '{model.Correo}'.");

        var dup = await _uow.ClienteRepository.ObtenerPorIdentificacionAsync(
            model.TipoIdentificacion, model.NumeroIdentificacion, ct);
        if (dup is not null && dup.ClienteGuid != clienteGuid)
            throw new InvalidOperationException("Ya existe otro cliente con el mismo tipo y número de identificación.");

        ClienteDataMapper.ApplyUpdate(e, model);
        e.FechaModificacionUtc = DateTimeOffset.UtcNow;
        _uow.ClienteRepository.Actualizar(e);
        await _uow.SaveChangesAsync(ct);

        var reloaded = await _uow.ClienteRepository.ObtenerPorGuidAsync(clienteGuid, ct);
        return reloaded is null ? null : ClienteDataMapper.ToDataModel(reloaded);
    }

    public async Task<bool> InhabilitarAsync(Guid clienteGuid, string motivo, string usuario, CancellationToken ct = default)
    {
        var e = await _uow.ClienteRepository.ObtenerParaActualizarAsync(clienteGuid, ct);
        if (e is null || e.Estado == "INA") return false;

        e.Estado = "INA";
        e.MotivoInhabilitacion = motivo;
        e.FechaInhabilitacionUtc = DateTimeOffset.UtcNow;
        e.ModificadoPorUsuario = usuario;
        e.FechaModificacionUtc = DateTimeOffset.UtcNow;
        _uow.ClienteRepository.Actualizar(e);
        await _uow.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> EliminarLogicoAsync(Guid clienteGuid, string usuario, CancellationToken ct = default)
    {
        var e = await _uow.ClienteRepository.ObtenerParaActualizarAsync(clienteGuid, ct);
        if (e is null || e.EsEliminado) return false;

        e.EsEliminado = true;
        e.Estado = "INA";
        e.MotivoInhabilitacion = "Eliminación lógica (API)";
        e.ModificadoPorUsuario = usuario;
        e.FechaModificacionUtc = DateTimeOffset.UtcNow;
        _uow.ClienteRepository.Actualizar(e);
        await _uow.SaveChangesAsync(ct);
        return true;
    }
}
