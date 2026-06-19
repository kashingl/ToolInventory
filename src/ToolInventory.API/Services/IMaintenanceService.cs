using ToolInventory.Core.Entities;
using ToolInventory.Shared.DTOs;

namespace ToolInventory.API.Services;

public interface IMaintenanceService
{
    Task<ServiceResult<MaintenanceRecord>> CreateAsync(CreateMaintenanceRecordDto dto, CancellationToken cancellationToken = default);
    Task<ServiceResult<MaintenanceRecord>> CompleteAsync(int id, CancellationToken cancellationToken = default);
    Task<ServiceResult<MaintenanceRecord>> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
