using ToolInventory.Core.Entities;
using ToolInventory.Shared.DTOs;

namespace ToolInventory.API.Services;

public interface ICheckoutService
{
    Task<ServiceResult<Checkout>> CreateAsync(CreateCheckoutDto dto, CancellationToken cancellationToken = default);
    Task<ServiceResult<Checkout>> CheckInAsync(int checkoutId, CancellationToken cancellationToken = default);
    Task<ServiceResult<Checkout>> CheckInByToolIdAsync(int toolId, CancellationToken cancellationToken = default);
}
