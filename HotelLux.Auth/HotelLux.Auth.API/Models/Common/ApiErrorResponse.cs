namespace HotelLux.Auth.API.Models.Common;

public class ApiErrorResponse
{
    public int Status { get; set; }
    public string Error { get; set; } = null!;
    public IReadOnlyCollection<string> Details { get; set; } = Array.Empty<string>();
    public string Timestamp { get; set; } = DateTime.UtcNow.ToString("O");

    public static ApiErrorResponse Fail(int status, string error, IReadOnlyCollection<string>? details = null)
    {
        return new ApiErrorResponse
        {
            Status = status,
            Error = error,
            Details = details ?? Array.Empty<string>(),
            Timestamp = DateTime.UtcNow.ToString("O")
        };
    }
}
