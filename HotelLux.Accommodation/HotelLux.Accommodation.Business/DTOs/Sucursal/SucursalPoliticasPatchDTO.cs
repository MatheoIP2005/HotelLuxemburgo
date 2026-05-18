namespace HotelLux.Accommodation.Business.DTOs.Sucursal;

/// <summary>Actualización parcial solo de políticas operativas (spec PATCH .../politicas).</summary>
public class SucursalPoliticasPatchDTO
{
    public string? HoraCheckin { get; set; }
    public string? HoraCheckout { get; set; }
    public bool? AceptaNinos { get; set; }
    public int? EdadMinimaHuesped { get; set; }
    public bool? PermiteMascotas { get; set; }
    public bool? SePermiteFumar { get; set; }
    public bool? CheckinAnticipado { get; set; }
    public bool? CheckoutTardio { get; set; }
    /// <summary>Texto libre de políticas (se persiste en descripción corta si se envía).</summary>
    public string? Politicas { get; set; }
    public string? ModificadoPorUsuario { get; set; }
    public string? ModificadoDesdeIp { get; set; }
}
