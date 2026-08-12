namespace SmartPOS.Services;

public interface IInvoicePathProvider
{
    string GetInvoicePath(int orderId, DateTime orderDate);
}
