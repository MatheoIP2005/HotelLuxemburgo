namespace HotelLux.Accommodation.API.Models.Common;

public class ApiErrorResponse
{
    public int Status { get; set; }
    public string Error { get; set; } = null!;
    public IReadOnlyList<string>? Details { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiErrorResponse Fail(int status, string error, IReadOnlyList<string>? details = null)
        => new() { Status = status, Error = error, Details = details };
}
