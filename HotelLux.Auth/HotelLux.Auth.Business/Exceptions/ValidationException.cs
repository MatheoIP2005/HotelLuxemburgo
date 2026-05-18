namespace HotelLux.Auth.Business.Exceptions;

public class ValidationException : BusinessException
{
    public IReadOnlyCollection<string> Errors { get; }

    public ValidationException(string message, IReadOnlyCollection<string>? errors = null)
        : base(message)
    {
        Errors = errors ?? Array.Empty<string>();
    }
}
