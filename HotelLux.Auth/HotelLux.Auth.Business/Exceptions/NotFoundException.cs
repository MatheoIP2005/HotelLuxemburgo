namespace HotelLux.Auth.Business.Exceptions;

public class NotFoundException : BusinessException
{
    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string entidad, object id)
        : base($"No se encontró {entidad} con identificador '{id}'.")
    {
    }
}
