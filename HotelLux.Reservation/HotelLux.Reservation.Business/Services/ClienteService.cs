using System.Text.Json;
using HotelLux.Reservation.Business.DTOs.Cliente;
using HotelLux.Reservation.Business.DTOs.Common;
using HotelLux.Reservation.Business.Exceptions;
using HotelLux.Reservation.Business.Interfaces;
using HotelLux.Reservation.Business.Mappers;
using HotelLux.Reservation.DataManagement.Interfaces;

namespace HotelLux.Reservation.Business.Services;

public class ClienteService : IClienteService
{
    private readonly IClienteDataService _data;
    private readonly IAuditEmitter _audit;

    public ClienteService(IClienteDataService data, IAuditEmitter audit)
    {
        _data = data;
        _audit = audit;
    }

    public async Task<ClienteDto?> ObtenerPorGuidAsync(Guid clienteGuid, CancellationToken ct = default)
    {
        var m = await _data.ObtenerPorGuidAsync(clienteGuid, ct);
        return m is null ? null : ClienteBusinessMapper.ToDto(m);
    }

    public async Task<PagedResultDTO<ClienteDto>> ListarAsync(int pagina, int limite, CancellationToken ct = default)
    {
        var p = pagina < 1 ? 1 : pagina;
        var l = limite < 1 ? 20 : Math.Min(limite, 200);
        var page = await _data.ListarAsync(p, l, ct);
        return new PagedResultDTO<ClienteDto>
        {
            Items = page.Items.Select(ClienteBusinessMapper.ToDto).ToList(),
            Total = page.Total,
            Pagina = p,
            Limite = l
        };
    }

    public async Task<ClienteDto> CrearAsync(ClienteCreateDto dto, CancellationToken ct = default)
    {
        var creadoPor = dto.CreadoPorUsuario ?? "api_user";
        var model = ClienteBusinessMapper.ToDataModel(dto, creadoPor);
        try
        {
            var created = await _data.CrearAsync(model, ct);
            _audit.EmitFireAndForget(
                "reservation-service",
                "reservas.cliente",
                "INSERT",
                created.ClienteGuid.ToString(),
                created.IdCliente.ToString(),
                Guid.Empty.ToString(),
                creadoPor,
                null,
                null,
                JsonSerializer.Serialize(created));
            return ClienteBusinessMapper.ToDto(created);
        }
        catch (InvalidOperationException ex)
        {
            throw new ConflictException("Cliente", ex.Message);
        }
    }

    public async Task<ClienteDto> ActualizarAsync(Guid clienteGuid, ClienteUpdateDto dto, CancellationToken ct = default)
    {
        var mod = dto.ModificadoPorUsuario ?? "api_user";
        var model = ClienteBusinessMapper.ToDataModel(clienteGuid, dto, mod);
        try
        {
            var updated = await _data.ActualizarAsync(clienteGuid, model, ct);
            if (updated is null) throw new NotFoundException("Cliente", clienteGuid);

            _audit.EmitFireAndForget(
                "reservation-service",
                "reservas.cliente",
                "UPDATE",
                updated.ClienteGuid.ToString(),
                updated.IdCliente.ToString(),
                Guid.Empty.ToString(),
                mod,
                null,
                null,
                JsonSerializer.Serialize(updated));

            return ClienteBusinessMapper.ToDto(updated);
        }
        catch (InvalidOperationException ex)
        {
            throw new ConflictException("Cliente", ex.Message);
        }
    }

    public async Task InhabilitarAsync(Guid clienteGuid, string motivo, string usuario, CancellationToken ct = default)
    {
        var ok = await _data.InhabilitarAsync(clienteGuid, motivo, usuario, ct);
        if (!ok) throw new NotFoundException("Cliente", clienteGuid);

        _audit.EmitFireAndForget(
            "reservation-service",
            "reservas.cliente",
            "UPDATE",
            clienteGuid.ToString(),
            clienteGuid.ToString(),
            Guid.Empty.ToString(),
            usuario,
            null,
            null,
            JsonSerializer.Serialize(new { estado = "INA", motivo }));
    }

    public async Task EliminarLogicoAsync(Guid clienteGuid, string usuario, CancellationToken ct = default)
    {
        var ok = await _data.EliminarLogicoAsync(clienteGuid, usuario, ct);
        if (!ok) throw new NotFoundException("Cliente", clienteGuid);

        _audit.EmitFireAndForget(
            "reservation-service",
            "reservas.cliente",
            "UPDATE",
            clienteGuid.ToString(),
            clienteGuid.ToString(),
            Guid.Empty.ToString(),
            usuario,
            null,
            null,
            JsonSerializer.Serialize(new { esEliminado = true }));
    }
}
