namespace ToolInventory.Core.Entities;

public class Tool
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Make { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public string? Owner { get; set; }
    public string? Description { get; set; }
    public string? Barcode { get; set; }
    public string? Location { get; set; }
    public string? Systainer { get; set; }
    public ToolStatus Status { get; set; } = ToolStatus.Available;
    public string? ImageUrl { get; set; }
    public int? CategoryId { get; set; }
    public Category? Category { get; set; }
    public ICollection<Checkout> Checkouts { get; set; } = new List<Checkout>();
    public ICollection<MaintenanceRecord> MaintenanceRecords { get; set; } = new List<MaintenanceRecord>();
}
