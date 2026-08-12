namespace SmartPOS.Models;
public class FaceSample { public int FaceSampleId { get; set; } public int EmployeeId { get; set; } public string ImagePath { get; set; } = string.Empty; public DateTime CreatedDate { get; set; } public Employee Employee { get; set; } = null!; }
