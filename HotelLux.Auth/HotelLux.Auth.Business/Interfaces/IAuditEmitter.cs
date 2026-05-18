namespace HotelLux.Auth.Business.Interfaces;

public interface IAuditEmitter
{
    Task EmitAsync(
        string tablaAfectada,
        string operacion,
        string entidadGuid,
        string usuarioGuid,
        string detalleJson,
        CancellationToken cancellationToken = default);
}
