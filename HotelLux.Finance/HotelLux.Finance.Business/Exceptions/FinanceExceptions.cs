namespace HotelLux.Finance.Business.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string entidad, Guid guid)
        : base($"{entidad} con GUID '{guid}' no encontrado.") { }
}

public class ValidationException : Exception
{
    public ValidationException(string message, IReadOnlyList<string> errors) : base(message)
        => Errors = errors;

    public IReadOnlyList<string> Errors { get; }
}

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}
