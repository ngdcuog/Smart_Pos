using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace SmartPOS.Services;

public sealed class AIChatService(IReportService reports, HttpClient http, AISettings settings) : IAIChatService
{
    public async Task<string> AskAsync(string question, CancellationToken token = default)
    {
        var apiKey = string.IsNullOrWhiteSpace(settings.ApiKey)
            ? Environment.GetEnvironmentVariable(settings.ApiKeyEnvironmentVariable)
            : settings.ApiKey;
        if (string.IsNullOrWhiteSpace(apiKey)) throw new InvalidOperationException("Trợ lý AI chưa được cấu hình API key.");
        var context = await BuildContextAsync(question);
        var prompt = $"Bạn là trợ lý SmartPOS. Trả lời tiếng Việt, ngắn gọn. Chỉ dùng số liệu trong CONTEXT, không tự bịa số liệu hay nguyên nhân.\nCÂU HỎI: {question}\nCONTEXT:\n{context}";
        var payload = JsonSerializer.Serialize(new { contents = new[] { new { parts = new[] { new { text = prompt } } } } });
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{settings.BaseUrl}?key={Uri.EscapeDataString(apiKey)}") { Content = new StringContent(payload, Encoding.UTF8, "application/json") };
        using var response = await http.SendAsync(request, token);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException("Trợ lý AI hiện không khả dụng.");
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(token));
        return json.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString() ?? "Không nhận được câu trả lời.";
    }

    private async Task<string> BuildContextAsync(string question)
    {
        var dashboard = await reports.GetDashboardAsync();
        var report = await reports.GetSalesReportAsync(DateTime.Today.AddDays(-6), DateTime.Today);
        var q = question.ToLowerInvariant();
        if (q.Contains("sắp hết") || q.Contains("tồn kho")) return string.Join('\n', dashboard.LowStockProducts.Select(x => $"{x.ProductName}: tồn={x.StockQuantity}, tối thiểu={x.MinStockAlert}"));
        if (q.Contains("bán chạy") || q.Contains("top sản phẩm")) return string.Join('\n', report.TopProducts.Take(5).Select(x => $"{x.ProductName}: quantity={x.QuantitySold}, revenue={x.Revenue}"));
        if (q.Contains("danh mục")) return string.Join('\n', report.CategorySales.Select(x => $"{x.CategoryName}: quantity={x.QuantitySold}, revenue={x.Revenue}"));
        if (q.Contains("chấm công")) return $"AttendanceToday={dashboard.AttendanceToday}";
        return $"Date={DateTime.Today:yyyy-MM-dd}; RevenueToday={dashboard.RevenueToday}; OrdersToday={dashboard.OrdersToday}; RevenueLast7Days={report.TotalRevenue}; OrdersLast7Days={report.TotalOrders}; UnitsSoldLast7Days={report.UnitsSold}";
    }
}
