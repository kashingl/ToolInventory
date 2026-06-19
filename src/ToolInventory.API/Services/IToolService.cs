using ToolInventory.Core.Entities;
using ToolInventory.Shared.DTOs;

namespace ToolInventory.API.Services;

public interface IToolService
{
    Task<ServiceResult<Tool>> CreateAsync(CreateToolDto dto, CancellationToken cancellationToken = default);
    Task<ServiceResult<Tool>> UpdateAsync(int id, UpdateToolDto dto, CancellationToken cancellationToken = default);
}
