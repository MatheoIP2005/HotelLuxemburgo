namespace HotelLux.Gateway.GraphQL.Types;

public class AccommodationSearchItem
{
    public Guid SucursalGuid { get; set; }
    public string? Nombre { get; set; }
    public string? Ciudad { get; set; }
    public string? Provincia { get; set; }
    public string? Pais { get; set; }
    public string? Direccion { get; set; }
    public string? Descripcion { get; set; }
    public string? Categoria { get; set; }
    public int Estrellas { get; set; }
    public string? TipoAlojamiento { get; set; }
    public decimal PrecioDesde { get; set; }
    public string? Moneda { get; set; }
    public string? ImagenPrincipalUrl { get; set; }
    public double PromedioValoracion { get; set; }
    public int TotalValoraciones { get; set; }
    public int HabitacionesDisponibles { get; set; }
    public IList<string>? ServiciosDestacados { get; set; }
    public string? HoraCheckIn { get; set; }
    public string? HoraCheckOut { get; set; }
    public bool AceptaNinos { get; set; }
    public bool PermiteMascotas { get; set; }
}

public class AccommodationSearchPagedResult
{
    public IList<AccommodationSearchItem> Items { get; set; } = [];
    public int Pagina { get; set; }
    public int Limite { get; set; }
    public int TotalResultados { get; set; }
    public int TotalPaginas { get; set; }
    public bool TieneSiguiente { get; set; }
    public bool TieneAnterior { get; set; }
}

public class AccommodationDetailResult : AccommodationSearchItem
{
    public string? DescripcionCompleta { get; set; }
    public IList<string>? Imagenes { get; set; }
    public IList<string>? Amenities { get; set; }
}

public class AccommodationReview
{
    public Guid ValoracionGuid { get; set; }
    public int Puntuacion { get; set; }
    public string? ComentarioPositivo { get; set; }
    public string? ComentarioNegativo { get; set; }
    public string? TipoViaje { get; set; }
    public DateTimeOffset Fecha { get; set; }
    public string? NombreVisibleCliente { get; set; }
    public string? RespuestaPropiedad { get; set; }
}

public class AccommodationReviewsPagedResult
{
    public IList<AccommodationReview> Items { get; set; } = [];
    public int Pagina { get; set; }
    public int Limite { get; set; }
    public int TotalResultados { get; set; }
    public int TotalPaginas { get; set; }
    public bool TieneSiguiente { get; set; }
    public bool TieneAnterior { get; set; }
}

public class ReservationResult
{
    public Guid ReservaGuid { get; set; }
    public string? CodigoReserva { get; set; }
    public Guid? ClienteGuid { get; set; }
    public Guid SucursalGuid { get; set; }
    public DateTimeOffset? FechaReservaUtc { get; set; }
    public DateTimeOffset FechaInicio { get; set; }
    public DateTimeOffset FechaFin { get; set; }
    public decimal SubtotalReserva { get; set; }
    public decimal ValorIva { get; set; }
    public decimal TotalReserva { get; set; }
    public decimal SaldoPendiente { get; set; }
    public string? OrigenCanalReserva { get; set; }
    public string? EstadoReserva { get; set; }
    public DateTimeOffset? FechaConfirmacionUtc { get; set; }
    public string? Observaciones { get; set; }
}

public class CreateReservationInput
{
    public Guid SucursalGuid { get; set; }
    public DateTimeOffset FechaInicio { get; set; }
    public DateTimeOffset FechaFin { get; set; }
    public string? OrigenCanalReserva { get; set; }
    public string? Observaciones { get; set; }
    public bool EsWalkin { get; set; }
    public ClienteInlineInput? Cliente { get; set; }
    public IList<ReservaHabitacionInput> Habitaciones { get; set; } = [];
}

public class ClienteInlineInput
{
    public string TipoIdentificacion { get; set; } = string.Empty;
    public string NumeroIdentificacion { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
}

public class ReservaHabitacionInput
{
    public Guid TipoHabitacionGuid { get; set; }
    public int NumHabitaciones { get; set; }
    public int NumAdultos { get; set; }
    public int NumNinos { get; set; }
}
