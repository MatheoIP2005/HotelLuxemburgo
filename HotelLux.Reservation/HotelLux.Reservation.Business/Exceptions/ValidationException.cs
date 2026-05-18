namespace HotelLux.Reservation.Business.Exceptions;

public class ValidationException : Exception
{
    public IReadOnlyList<string> Errors { get; }
    public ValidationException(string message, IReadOnlyList<string>? errors = null)
        : base(message) => Errors = errors ?? Array.Empty<string>();
}
