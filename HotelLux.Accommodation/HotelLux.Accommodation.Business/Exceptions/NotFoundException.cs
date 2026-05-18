namespace HotelLux.Accommodation.Business.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string entidad, object id)
        : base($"{entidad} con identificador '{id}' no fue encontrado.") { }
}
