using System.ComponentModel.DataAnnotations;

namespace HotelLux.Stay.Business.DTOs;

public class CargoEstadiaCreateDto
{
    public Guid? CatalogoGuid { get; set; }

    [Required, MaxLength(250)]
    public string DescripcionCargo { get; set; } = null!;

    [Range(1, int.MaxValue)]
    public int Cantidad { get; set; } = 1;

    [Range(0, double.MaxValue)]
    public decimal PrecioUnitario { get; set; }

    [Range(0, double.MaxValue)]
    public decimal ValorIva { get; set; }

    public string? CreadoPorUsuario { get; set; }
}

public class CargoEstadiaDto
{
    public Guid CargoGuid { get; set; }
    public Guid EstadiaGuid { get; set; }
    public Guid? CatalogoGuid { get; set; }
    public string DescripcionCargo { get; set; } = null!;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
    public decimal ValorIva { get; set; }
    public decimal TotalCargo { get; set; }
    public DateTimeOffset FechaConsumoUtc { get; set; }
    public string EstadoCargo { get; set; } = null!;
}
