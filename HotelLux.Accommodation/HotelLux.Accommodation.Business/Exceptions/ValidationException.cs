namespace HotelLux.Accommodation.Business.Exceptions;

public class ValidationException : Exception
{
    public IReadOnlyList<string> Errors { get; }
    public ValidationException(string message, IReadOnlyList<string> errors)
        : base(message) => Errors = errors;
}
