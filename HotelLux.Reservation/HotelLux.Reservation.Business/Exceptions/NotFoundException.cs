namespace HotelLux.Reservation.Business.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string entidad, Guid guid)
        : base($"{entidad} con GUID '{guid}' no encontrado.") { }

    public NotFoundException(string entidad, string codigo)
        : base($"{entidad} con código '{codigo}' no encontrado.") { }
}
