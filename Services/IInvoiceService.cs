using SmartPOS.Services.Dtos;

namespace SmartPOS.Services;

public interface IInvoiceService
{
    Task<InvoicePdfResult> GeneratePdfAsync(int orderId, CancellationToken cancellationToken = default);
    void OpenPdf(string filePath);
}
