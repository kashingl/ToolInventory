namespace ToolInventory.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<Entities.Tool> Tools { get; }
    IRepository<Entities.Category> Categories { get; }
    IRepository<Entities.Checkout> Checkouts { get; }
    IRepository<Entities.MaintenanceRecord> MaintenanceRecords { get; }
    Task<Entities.Checkout?> GetLatestActiveCheckoutByToolIdAsync(int toolId, CancellationToken cancellationToken = default);
    Task<bool> HasOtherMaintenanceRecordsForToolAsync(int toolId, int excludedRecordId, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
