namespace ToolInventory.Core.Entities;

public class Checkout
{
    public int Id { get; set; }
    public int ToolId { get; set; }
    public Tool Tool { get; set; } = null!;
    public string UserId { get; set; } = string.Empty;
    public DateTime CheckoutDate { get; set; }
    public DateTime? ExpectedReturnDate { get; set; }
    public DateTime? ActualReturnDate { get; set; }
    public string? Notes { get; set; }
}
