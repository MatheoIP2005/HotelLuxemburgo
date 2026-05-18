namespace HotelLux.Finance.Business.DTOs;

public class FacturaDto
{
    public Guid FacturaGuid { get; set; }
    public Guid ClienteGuid { get; set; }
    public Guid ReservaGuid { get; set; }
    public Guid SucursalGuid { get; set; }
    public string NumeroFactura { get; set; } = null!;
    public string TipoFactura { get; set; } = null!;
    public decimal Total { get; set; }
    public decimal SaldoPendiente { get; set; }
    public string Estado { get; set; } = null!;
}

public class PagoCreateDto
{
    public Guid FacturaGuid { get; set; }
    public decimal Monto { get; set; }
    public string MetodoPago { get; set; } = null!;
    public string? CreadoPorUsuario { get; set; }
}

public class AnularFacturaDto
{
    public string Motivo { get; set; } = null!;
}

/// <summary>Línea de detalle para generación manual de facturas (REST o gRPC).</summary>
public class FacturaLineaGeneracionDto
{
    public string Descripcion { get; set; } = null!;
    public int Cantidad { get; set; } = 1;
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
    public decimal ValorIva { get; set; }
    public decimal Descuento { get; set; }
    public decimal Total { get; set; }
    public string? TipoItem { get; set; }
    public string? ReferenciaTipo { get; set; }
    public Guid? ReferenciaGuid { get; set; }
}

/// <summary>Cuerpo para POST generar-reserva / generar-final / final-y-pago-simulado.</summary>
public class GenerarFacturaRequestDto
{
    public Guid ClienteGuid { get; set; }
    public Guid SucursalGuid { get; set; }
    public List<FacturaLineaGeneracionDto> Items { get; set; } = new();
}

public class PagoEstadoDto
{
    public string NuevoEstado { get; set; } = null!;
    public byte[]? RowVersion { get; set; }
}
