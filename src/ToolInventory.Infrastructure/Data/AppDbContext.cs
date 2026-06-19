using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ToolInventory.Core.Entities;

namespace ToolInventory.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<IdentityUser>(options)
{
    public DbSet<Tool> Tools => Set<Tool>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Checkout> Checkouts => Set<Checkout>();
    public DbSet<MaintenanceRecord> MaintenanceRecords => Set<MaintenanceRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Tool>(e =>
        {
            e.Property(t => t.Name).IsRequired().HasMaxLength(200);
            e.Property(t => t.Barcode).HasMaxLength(100);
            e.Property(t => t.Location).HasMaxLength(200);
            e.Property(t => t.Systainer).HasMaxLength(100);
            e.Property(t => t.Status).HasConversion<string>();
            e.HasIndex(t => t.Barcode).IsUnique().HasFilter("[Barcode] IS NOT NULL");
            e.HasIndex(t => t.CategoryId);
        });

        modelBuilder.Entity<Category>(e =>
        {
            e.Property(c => c.Name).IsRequired().HasMaxLength(100);
            e.HasIndex(c => c.Name).IsUnique();
        });

        modelBuilder.Entity<Checkout>(e =>
        {
            e.HasOne(c => c.Tool).WithMany(t => t.Checkouts).HasForeignKey(c => c.ToolId);
            e.Property(c => c.UserId).IsRequired().HasMaxLength(450);
            e.HasIndex(c => c.UserId);
            e.HasIndex(c => c.ToolId);
            e.ToTable(t => t.HasCheckConstraint("CK_Checkouts_ReturnDate", "[ActualReturnDate] IS NULL OR [ActualReturnDate] >= [CheckoutDate]"));
        });

        modelBuilder.Entity<MaintenanceRecord>(e =>
        {
            e.Property(m => m.Cost).HasPrecision(18, 2);
            e.HasOne(m => m.Tool).WithMany(t => t.MaintenanceRecords).HasForeignKey(m => m.ToolId);
            e.HasIndex(m => m.ToolId);
            e.ToTable(t => t.HasCheckConstraint("CK_MaintenanceRecord_Cost", "[Cost] IS NULL OR [Cost] >= 0"));
        });
    }
}
