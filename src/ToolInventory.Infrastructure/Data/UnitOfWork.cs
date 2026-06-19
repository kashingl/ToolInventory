using ToolInventory.Core.Entities;
using ToolInventory.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ToolInventory.Infrastructure.Data;

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    private IRepository<Tool>? _tools;
    private IRepository<Category>? _categories;
    private IRepository<Checkout>? _checkouts;
    private IRepository<MaintenanceRecord>? _maintenanceRecords;

    public IRepository<Tool> Tools => _tools ??= new Repository<Tool>(context);
    public IRepository<Category> Categories => _categories ??= new Repository<Category>(context);
    public IRepository<Checkout> Checkouts => _checkouts ??= new Repository<Checkout>(context);
    public IRepository<MaintenanceRecord> MaintenanceRecords => _maintenanceRecords ??= new Repository<MaintenanceRecord>(context);

    public Task<Checkout?> GetLatestActiveCheckoutByToolIdAsync(int toolId, CancellationToken cancellationToken = default)
        => context.Checkouts
            .Where(c => c.ToolId == toolId && !c.ActualReturnDate.HasValue)
            .OrderByDescending(c => c.CheckoutDate)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<bool> HasOtherMaintenanceRecordsForToolAsync(int toolId, int excludedRecordId, CancellationToken cancellationToken = default)
        => context.MaintenanceRecords
            .AnyAsync(m => m.ToolId == toolId && m.Id != excludedRecordId, cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => context.SaveChangesAsync(cancellationToken);
    public void Dispose() => context.Dispose();
}
