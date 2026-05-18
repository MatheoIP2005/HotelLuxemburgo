namespace HotelLux.Accommodation.API.Models.Common;

public class ApiResponse<T>
{
    public int Status { get; set; }
    public string Message { get; set; } = null!;
    public T? Data { get; set; }

    public static ApiResponse<T> Ok(T? data, string message = "Consulta exitosa.")
        => new() { Status = 200, Message = message, Data = data };

    public static ApiResponse<T> Created(T? data, string message = "Recurso creado exitosamente.")
        => new() { Status = 201, Message = message, Data = data };
}
