namespace HotelLux.Reservation.Business.DTOs.Cliente;

public class ClienteDto
{
    public Guid ClienteGuid { get; set; }
    public string TipoIdentificacion { get; set; } = null!;
    public string NumeroIdentificacion { get; set; } = null!;
    public string Nombres { get; set; } = null!;
    public string? Apellidos { get; set; }
    public string? RazonSocial { get; set; }
    public string Correo { get; set; } = null!;
    public string Telefono { get; set; } = null!;
    public string Direccion { get; set; } = null!;
    public string Estado { get; set; } = null!;
    public DateTimeOffset FechaRegistroUtc { get; set; }
}

public class ClienteCreateDto
{
    public string TipoIdentificacion { get; set; } = null!;
    public string NumeroIdentificacion { get; set; } = null!;
    public string Nombres { get; set; } = null!;
    public string? Apellidos { get; set; }
    public string? RazonSocial { get; set; }
    public string Correo { get; set; } = null!;
    public string Telefono { get; set; } = null!;
    public string Direccion { get; set; } = null!;
    public string? CreadoPorUsuario { get; set; }
}

public class ClienteUpdateDto
{
    public string TipoIdentificacion { get; set; } = null!;
    public string NumeroIdentificacion { get; set; } = null!;
    public string Nombres { get; set; } = null!;
    public string? Apellidos { get; set; }
    public string? RazonSocial { get; set; }
    public string Correo { get; set; } = null!;
    public string Telefono { get; set; } = null!;
    public string Direccion { get; set; } = null!;
    public string? ModificadoPorUsuario { get; set; }
}

public class InhabilitarDto
{
    public string Motivo { get; set; } = null!;
}
