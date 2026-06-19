using System.ComponentModel.DataAnnotations;

namespace ToolInventory.Shared.DTOs;

public class MaintenanceRecordDto
{
    public int Id { get; set; }
    public int ToolId { get; set; }
    public string ToolName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? PerformedBy { get; set; }
    public decimal? Cost { get; set; }
    public DateTime? NextMaintenanceDate { get; set; }
}

public class CreateMaintenanceRecordDto
{
    [Range(1, int.MaxValue)]
    public int ToolId { get; set; }

    public DateTime Date { get; set; }

    [Required]
    [StringLength(2000, MinimumLength = 3)]
    public string Description { get; set; } = string.Empty;

    [StringLength(200)]
    public string? PerformedBy { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal? Cost { get; set; }
    public DateTime? NextMaintenanceDate { get; set; }
}
