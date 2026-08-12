using SmartPOS.Services.Dtos;
namespace SmartPOS.Services;
public interface IReportService { Task<DashboardDataDto> GetDashboardAsync(); Task<SalesReportDto> GetSalesReportAsync(DateTime fromDate, DateTime toDate); }
