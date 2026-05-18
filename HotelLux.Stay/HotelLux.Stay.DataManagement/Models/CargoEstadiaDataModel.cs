namespace HotelLux.Stay.DataManagement.Models;

public class CargoEstadiaDataModel
{
    public int IdCargoEstadia { get; set; }
    public Guid CargoGuid { get; set; }
    public int IdEstadia { get; set; }
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
    public DateTimeOffset FechaRegistroUtc { get; set; }
    public string CreadoPorUsuario { get; set; } = null!;
}
