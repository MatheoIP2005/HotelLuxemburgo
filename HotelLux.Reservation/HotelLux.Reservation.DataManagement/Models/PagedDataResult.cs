namespace HotelLux.Reservation.DataManagement.Models;

public class PagedDataResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
    public int Total { get; set; }
    public int Pagina { get; set; }
    public int Limite { get; set; }
}
