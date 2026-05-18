namespace HotelLux.Accommodation.Business.Interfaces;

public interface IAuditEmitter
{
    void EmitFireAndForget(
        string servicioOrigen,
        string tablaAfectada,
        string operacion,
        string entidadGuid,
        string? idRegistro,
        string usuarioGuid,
        string usuarioEjecutor,
        string? ipOrigen,
        string? datosAnterioresJson = null,
        string? datosNuevosJson = null);
}
