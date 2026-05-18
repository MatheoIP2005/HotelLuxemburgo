namespace HotelLux.Stay.Business.Interfaces;

public interface IFinanceStayClient
{
    Task<bool> GenerateFinalInvoiceAsync(
        Guid estadiaGuid,
        Guid reservaGuid,
        Guid clienteGuid,
        Guid sucursalGuid,
        string creadoPorUsuario,
        CancellationToken ct = default);
}
