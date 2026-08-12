using SmartPOS.Models.Enums;
namespace SmartPOS.Models;
public class Order { public int OrderId { get; set; } public int EmployeeId { get; set; } public DateTime OrderDate { get; set; } public decimal TotalAmount { get; set; } public decimal DiscountAmount { get; set; } public decimal FinalAmount { get; set; } public PaymentMethod PaymentMethod { get; set; } public Employee Employee { get; set; } = null!; public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>(); }
