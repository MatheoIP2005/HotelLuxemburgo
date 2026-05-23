namespace HotelLux.Accommodation.API.Models.Marketplace;

public sealed class AccommodationAvailabilityDto
{
    public DateTime? FechaEntrada { get; set; }
    public DateTime? FechaSalida { get; set; }
    public IList<AvailabilityByRoomTypeDto>? PorTipoHabitacion { get; set; }
}

public sealed class AvailabilityByRoomTypeDto
{
    public Guid TipoHabitacionGuid { get; set; }
    public string? Nombre { get; set; }
    public int Disponibles { get; set; }
}

public sealed class AccommodationCategoryDto
{
    public string? IdCategoria { get; set; }
    public string? NombreCategoria { get; set; }
    public int TotalPropiedades { get; set; }
    public decimal PrecioPromedioNoche { get; set; }
    public string? Moneda { get; set; }
}

public sealed class AccommodationPolicyDto
{
    public string? HoraCheckIn { get; set; }
    public string? HoraCheckOut { get; set; }
    public bool AceptaNinos { get; set; }
    public bool PermiteMascotas { get; set; }
    public string? Politicas { get; set; }
}

public sealed class AccommodationRoomTypeDto
{
    public Guid TipoHabitacionGuid { get; set; }
    public string? Nombre { get; set; }
    public string? TipoCama { get; set; }
    public int CapacidadAdultos { get; set; }
    public int CapacidadNinos { get; set; }
    public decimal AreaM2 { get; set; }
    public decimal PrecioBase { get; set; }
    public IList<string>? Imagenes { get; set; }
    public int DisponiblesEnRango { get; set; }
}

public sealed class AccommodationTariffDto
{
    public Guid TarifaGuid { get; set; }
    public string? Nombre { get; set; }
    public decimal PrecioPorNoche { get; set; }
    public string? Moneda { get; set; }
    public DateTimeOffset FechaInicio { get; set; }
    public DateTimeOffset FechaFin { get; set; }
    public int MinNoches { get; set; }
    public Guid TipoHabitacionGuid { get; set; }
}

public sealed class AccommodationSearchItemDto
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

public sealed class AccommodationSearchItemDtoPagedResponse
{
    public IList<AccommodationSearchItemDto> Items { get; set; } = [];
    public int Pagina { get; set; }
    public int Limite { get; set; }
    public int TotalResultados { get; set; }
    public int TotalPaginas { get; set; }
    public bool TieneSiguiente { get; set; }
    public bool TieneAnterior { get; set; }
}

public sealed class AccommodationDetailResponse
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
    public string? DescripcionCompleta { get; set; }
    public IList<AccommodationRoomTypeDto>? TiposHabitacion { get; set; }
    public IList<AccommodationTariffDto>? TarifasActivas { get; set; }
    public IList<string>? Amenities { get; set; }
    public IList<string>? Imagenes { get; set; }
    public AccommodationPolicyDto? Politicas { get; set; }
}

public sealed class AccommodationReviewDto
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

public sealed class AccommodationReviewDtoPagedResponse
{
    public IList<AccommodationReviewDto> Items { get; set; } = [];
    public int Pagina { get; set; }
    public int Limite { get; set; }
    public int TotalResultados { get; set; }
    public int TotalPaginas { get; set; }
    public bool TieneSiguiente { get; set; }
    public bool TieneAnterior { get; set; }
}
