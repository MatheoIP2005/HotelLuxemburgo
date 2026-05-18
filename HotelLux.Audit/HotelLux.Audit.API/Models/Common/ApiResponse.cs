namespace HotelLux.Audit.API.Models.Common;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = null!;
    public T? Data { get; set; }

    public static ApiResponse<T> Ok(T data, string message = "Operación exitosa.")
        => new() { Success = true, Message = message, Data = data };

    public static ApiResponse<T> Fail(string message)
        => new() { Success = false, Message = message, Data = default };
}
