using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ToolInventory.Shared.DTOs;

public class ToolDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Barcode { get; set; }
    public string? Location { get; set; }
    public string? Systainer { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
}

public class CreateToolDto
{
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [StringLength(100)]
    public string? Barcode { get; set; }

    [StringLength(200)]
    public string? Location { get; set; }

    [StringLength(100)]
    public string? Systainer { get; set; }

    [Url]
    [StringLength(2000)]
    public string? ImageUrl { get; set; }
    public int? CategoryId { get; set; }
}

public class UpdateToolDto : CreateToolDto
{
    [Required]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ToolStatusValue Status { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ToolStatusValue
{
    Available,
    CheckedOut,
    UnderMaintenance,
    Retired
}
