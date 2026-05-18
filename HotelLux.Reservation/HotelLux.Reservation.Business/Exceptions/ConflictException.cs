namespace HotelLux.Reservation.Business.Exceptions;

public class ConflictException : Exception
{
    public ConflictException(string entidad, string mensaje)
        : base($"Conflicto en {entidad}: {mensaje}") { }
}
