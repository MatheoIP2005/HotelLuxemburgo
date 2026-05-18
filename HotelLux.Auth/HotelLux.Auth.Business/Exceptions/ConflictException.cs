namespace HotelLux.Auth.Business.Exceptions;

public class ConflictException : BusinessException
{
    public ConflictException(string message) : base(message)
    {
    }

    public ConflictException(string entidad, string detalle)
        : base($"Conflicto en {entidad}: {detalle}")
    {
    }
}
