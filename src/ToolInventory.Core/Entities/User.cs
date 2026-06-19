namespace ToolInventory.Core.Entities;

public enum UserRole { Admin, Manager, Employee }

// Legacy entity retained for backward compatibility with historical migrations.
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Employee;
    public ICollection<Checkout> Checkouts { get; set; } = new List<Checkout>();
}
