namespace HotelLux.Reservation.Business.DTOs.Common;

public class PagedResultDTO<T>
{
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
    public int Total { get; set; }
    public int Pagina { get; set; }
    public int Limite { get; set; }
    public int TotalPaginas => Limite > 0 ? (int)Math.Ceiling((double)Total / Limite) : 0;
}
