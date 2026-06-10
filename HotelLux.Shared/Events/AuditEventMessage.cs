namespace HotelLux.Shared.Events;

public record AuditEventMessage
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public string ServicioOrigen { get; init; } = string.Empty;
    public string TablaAfectada { get; init; } = string.Empty;
    public string Operacion { get; init; } = string.Empty;
    public string EntidadGuid { get; init; } = string.Empty;
    public string? IdRegistro { get; init; }
    public string UsuarioGuid { get; init; } = string.Empty;
    public string UsuarioEjecutor { get; init; } = string.Empty;
    public string? IpOrigen { get; init; }
    public string? DatosAnterioresJson { get; init; }
    public string? DatosNuevosJson { get; init; }
    public DateTimeOffset FechaEventoUtc { get; init; } = DateTimeOffset.UtcNow;
}
