namespace HotelLux.Reservation.Business.Interfaces;

public interface IFinanceClient
{
    Task<bool> GenerateReservationInvoiceAsync(
        Guid reservaGuid,
        Guid clienteGuid,
        Guid sucursalGuid,
        decimal subtotal,
        decimal valorIva,
        decimal total,
        IEnumerable<(string Descripcion, decimal PrecioUnitario, decimal Cantidad, decimal Subtotal, decimal ValorIva, decimal Total)> lineas,
        string creadoPorUsuario,
        CancellationToken ct = default);
}
