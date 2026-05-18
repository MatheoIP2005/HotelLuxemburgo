namespace HotelLux.Reservation.API.Models.Common;

public class ApiErrorResponse
{
    public int Status { get; set; }
    public string Error { get; set; } = null!;
    public IReadOnlyList<string> Details { get; set; } = Array.Empty<string>();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
