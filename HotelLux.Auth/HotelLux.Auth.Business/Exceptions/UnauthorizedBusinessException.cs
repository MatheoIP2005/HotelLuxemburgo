namespace HotelLux.Auth.Business.Exceptions;

public class UnauthorizedBusinessException : BusinessException
{
    public UnauthorizedBusinessException(string message) : base(message)
    {
    }
}
