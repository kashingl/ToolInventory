using System.ComponentModel.DataAnnotations;

namespace ToolInventory.Shared.DTOs;

public class CheckoutDto
{
    public int Id { get; set; }
    public int ToolId { get; set; }
    public string ToolName { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime CheckoutDate { get; set; }
    public DateTime? ExpectedReturnDate { get; set; }
    public DateTime? ActualReturnDate { get; set; }
    public string? Notes { get; set; }
}

public class CreateCheckoutDto
{
    [Range(1, int.MaxValue)]
    public int ToolId { get; set; }

    [Required]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    public DateTime? ExpectedReturnDate { get; set; }

    [StringLength(2000)]
    public string? Notes { get; set; }
}
