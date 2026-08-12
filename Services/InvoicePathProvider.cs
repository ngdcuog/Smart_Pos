using System.IO;

namespace SmartPOS.Services;

public sealed class InvoicePathProvider : IInvoicePathProvider
{
    private readonly string _invoiceDirectory;

    public InvoicePathProvider()
        : this(GetDefaultInvoiceDirectory())
    {
    }

    public InvoicePathProvider(string invoiceDirectory)
    {
        _invoiceDirectory = Path.IsPathRooted(invoiceDirectory)
            ? invoiceDirectory
            : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, invoiceDirectory));
    }

    public string GetInvoicePath(int orderId, DateTime orderDate)
    {
        Directory.CreateDirectory(_invoiceDirectory);
        return Path.Combine(_invoiceDirectory, $"Hoa_don_{orderId:D6}_{orderDate:yyyyMMdd_HHmmss}.pdf");
    }

    private static string GetDefaultInvoiceDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "SmartPOS.sln")))
                return Path.Combine(directory.FullName, "Invoices");

            directory = directory.Parent;
        }

        return Path.Combine(AppContext.BaseDirectory, "Invoices");
    }
}
