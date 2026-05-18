namespace HotelLux.Finance.API.Models.Common;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = null!;
    public T? Data { get; set; }

    public static ApiResponse<T> Ok(T data, string message = "Operación exitosa.")
        => new() { Success = true, Message = message, Data = data };
    public static ApiResponse<T> Created(T data, string message = "Recurso creado.")
        => new() { Success = true, Message = message, Data = data };
    public static ApiResponse<T> Error(string message)
        => new() { Success = false, Message = message, Data = default };
}
