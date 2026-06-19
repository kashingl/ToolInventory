namespace ToolInventory.Core.Entities;

public class MaintenanceRecord
{
    public int Id { get; set; }
    public int ToolId { get; set; }
    public Tool Tool { get; set; } = null!;
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? PerformedBy { get; set; }
    public decimal? Cost { get; set; }
    public DateTime? NextMaintenanceDate { get; set; }
}
