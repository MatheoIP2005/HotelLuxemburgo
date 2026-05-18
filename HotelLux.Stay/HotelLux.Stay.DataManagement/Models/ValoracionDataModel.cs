namespace HotelLux.Stay.DataManagement.Models;

public class ValoracionDataModel
{
    public int    IdValoracion  { get; set; }
    public Guid   ValoracionGuid { get; set; }
    public Guid   EstadiaGuid   { get; set; }
    public Guid   SucursalGuid  { get; set; }
    public Guid   ClienteGuid   { get; set; }
    public decimal PuntuacionGeneral       { get; set; }
    public decimal PuntuacionLimpieza      { get; set; }
    public decimal PuntuacionConfort       { get; set; }
    public decimal PuntuacionUbicacion     { get; set; }
    public decimal PuntuacionInstalaciones { get; set; }
    public decimal PuntuacionPersonal      { get; set; }
    public decimal PuntuacionCalidadPrecio { get; set; }
    public string  ComentarioPositivo { get; set; } = null!;
    public string  ComentarioNegativo { get; set; } = null!;
    public string  TipoViaje          { get; set; } = null!;
    public DateTimeOffset FechaPublicacionUtc { get; set; }
    public string? RespuestaHotel { get; set; }
    public string? NombreVisibleCliente { get; set; }
    public bool    EsEliminado    { get; set; }
    public DateTimeOffset FechaRegistroUtc { get; set; }
    public string? CreadoPorUsuario { get; set; }
}

public class RatingSummaryDataModel
{
    public bool   TieneResenas          { get; set; }
    public double PromedioGeneral        { get; set; }
    public double PromedioLimpieza       { get; set; }
    public double PromedioConfort        { get; set; }
    public double PromedioUbicacion      { get; set; }
    public double PromedioInstalaciones  { get; set; }
    public double PromedioPersonal       { get; set; }
    public double PromedioCalidadPrecio  { get; set; }
    public int    TotalResenas           { get; set; }
}
