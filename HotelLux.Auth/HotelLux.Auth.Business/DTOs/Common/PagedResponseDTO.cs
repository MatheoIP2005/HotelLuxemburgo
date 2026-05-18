namespace HotelLux.Auth.Business.DTOs.Common;

public class PagedResponseDTO<T>
{
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
    public int Pagina { get; set; }
    public int TamanoPagina { get; set; }
    public int TotalRegistros { get; set; }
}
