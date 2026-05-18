namespace HotelLux.Stay.Business.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string entidad, Guid guid)
        : base($"{entidad} con GUID '{guid}' no encontrado.") { }
}

public class ValidationException : Exception
{
    public IReadOnlyList<string> Errors { get; }
    public ValidationException(string message, IReadOnlyList<string>? errors = null)
        : base(message) => Errors = errors ?? Array.Empty<string>();
}

public class ConflictException : Exception
{
    public ConflictException(string entidad, string mensaje)
        : base($"Conflicto en {entidad}: {mensaje}") { }
}
