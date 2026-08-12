using System.Diagnostics;
using System.IO;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SmartPOS.Data;
using SmartPOS.Models.Enums;
using SmartPOS.Services.Dtos;

namespace SmartPOS.Services;

public sealed class InvoiceService(IDbContextFactory<AppDbContext> contextFactory, IInvoicePathProvider invoicePathProvider) : IInvoiceService
{
    static InvoiceService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<InvoicePdfResult> GeneratePdfAsync(int orderId, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var order = await context.Orders
            .AsNoTracking()
            .Include(x => x.Employee)
            .Include(x => x.OrderDetails)
            .ThenInclude(x => x.Product)
            .SingleOrDefaultAsync(x => x.OrderId == orderId, cancellationToken)
            ?? throw new InvalidOperationException("Không tìm thấy đơn hàng để xuất hóa đơn.");

        var invoice = new InvoiceData(
            order.OrderId,
            order.OrderDate,
            order.Employee.FullName,
            GetPaymentMethodName(order.PaymentMethod),
            order.TotalAmount,
            order.DiscountAmount,
            order.FinalAmount,
            order.OrderDetails
                .OrderBy(x => x.OrderDetailId)
                .Select(x => new InvoiceLine(x.Product.ProductName, x.Quantity, x.UnitPrice, x.LineTotal))
                .ToList());

        var filePath = invoicePathProvider.GetInvoicePath(order.OrderId, order.OrderDate);
        await Task.Run(() => CreateDocument(invoice).GeneratePdf(filePath), cancellationToken);
        return new InvoicePdfResult(order.OrderId, filePath);
    }

    public void OpenPdf(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Không tìm thấy file hóa đơn.", filePath);

        Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
    }

    private static IDocument CreateDocument(InvoiceData invoice) => Document.Create(container =>
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(36);
            page.DefaultTextStyle(TextStyle.Default.FontFamily("Arial").FontSize(9));

            page.Header().Column(header =>
            {
                header.Item().Text("SMARTPOS").FontSize(20).Bold().FontColor(Colors.Blue.Darken2);
                header.Item().PaddingTop(3).Text("HÓA ĐƠN BÁN HÀNG").FontSize(13).SemiBold();
                header.Item().PaddingTop(4).Text($"Mã đơn: #{invoice.OrderId:D6}   •   {invoice.OrderDate:dd/MM/yyyy HH:mm}").FontColor(Colors.Grey.Darken1);
                header.Item().PaddingTop(3).Text($"Nhân viên: {invoice.EmployeeName}   •   Thanh toán: {invoice.PaymentMethod}").FontColor(Colors.Grey.Darken1);
                header.Item().PaddingTop(14).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
            });

            page.Content().PaddingTop(16).Column(content =>
            {
                content.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(5);
                        columns.ConstantColumn(42);
                        columns.ConstantColumn(86);
                        columns.ConstantColumn(94);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCell).Text("Sản phẩm");
                        header.Cell().Element(HeaderCell).AlignRight().Text("SL");
                        header.Cell().Element(HeaderCell).AlignRight().Text("Đơn giá");
                        header.Cell().Element(HeaderCell).AlignRight().Text("Thành tiền");
                    });

                    foreach (var line in invoice.Lines)
                    {
                        table.Cell().Element(BodyCell).Text(line.ProductName);
                        table.Cell().Element(BodyCell).AlignRight().Text(line.Quantity.ToString());
                        table.Cell().Element(BodyCell).AlignRight().Text(FormatMoney(line.UnitPrice));
                        table.Cell().Element(BodyCell).AlignRight().Text(FormatMoney(line.LineTotal));
                    }
                });

                content.Item().PaddingTop(16).AlignRight().Width(230).Column(summary =>
                {
                    summary.Item().Row(row =>
                    {
                        row.RelativeItem().Text("Tạm tính");
                        row.AutoItem().Text(FormatMoney(invoice.TotalAmount));
                    });
                    if (invoice.DiscountAmount > 0)
                        summary.Item().PaddingTop(5).Row(row =>
                        {
                            row.RelativeItem().Text("Giảm giá");
                            row.AutoItem().Text($"- {FormatMoney(invoice.DiscountAmount)}");
                        });
                    summary.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                    summary.Item().PaddingTop(10).Row(row =>
                    {
                        row.RelativeItem().Text("TỔNG THANH TOÁN").Bold();
                        row.AutoItem().Text(FormatMoney(invoice.FinalAmount)).FontSize(13).Bold().FontColor(Colors.Blue.Darken2);
                    });
                });
            });

            page.Footer().AlignCenter().Text("Cảm ơn quý khách đã mua hàng tại SmartPOS.").FontColor(Colors.Grey.Darken1);
        });
    });

    private static IContainer HeaderCell(IContainer container) => container
        .Background(Colors.Grey.Lighten3)
        .PaddingVertical(7)
        .PaddingHorizontal(6)
        .DefaultTextStyle(x => x.SemiBold());

    private static IContainer BodyCell(IContainer container) => container
        .BorderBottom(1)
        .BorderColor(Colors.Grey.Lighten3)
        .PaddingVertical(8)
        .PaddingHorizontal(6);

    private static string GetPaymentMethodName(PaymentMethod paymentMethod) => paymentMethod switch
    {
        PaymentMethod.BankTransfer => "Chuyển khoản",
        _ => "Tiền mặt"
    };

    private static string FormatMoney(decimal amount) => $"{amount:N0} ₫";

    private sealed record InvoiceData(
        int OrderId,
        DateTime OrderDate,
        string EmployeeName,
        string PaymentMethod,
        decimal TotalAmount,
        decimal DiscountAmount,
        decimal FinalAmount,
        IReadOnlyList<InvoiceLine> Lines);

    private sealed record InvoiceLine(string ProductName, int Quantity, decimal UnitPrice, decimal LineTotal);
}
