using ToolInventory.API.Common;
using ToolInventory.Core.Entities;
using ToolInventory.Core.Interfaces;
using ToolInventory.Shared.DTOs;

namespace ToolInventory.API.Services;

public class MaintenanceService(IUnitOfWork uow) : IMaintenanceService
{
    public async Task<ServiceResult<MaintenanceRecord>> CreateAsync(CreateMaintenanceRecordDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.Cost.HasValue && dto.Cost < 0)
        {
            return ServiceResult<MaintenanceRecord>.Fail(
                StatusCodes.Status400BadRequest,
                "Invalid maintenance cost.",
                "Cost cannot be negative.");
        }

        if (dto.NextMaintenanceDate.HasValue && dto.NextMaintenanceDate < dto.Date)
        {
            return ServiceResult<MaintenanceRecord>.Fail(
                StatusCodes.Status400BadRequest,
                "Invalid next maintenance date.",
                "Next maintenance date cannot be earlier than the maintenance date.");
        }

        var tool = await uow.Tools.GetByIdAsync(dto.ToolId, cancellationToken);
        if (tool is null)
        {
            return ServiceResult<MaintenanceRecord>.Fail(
                StatusCodes.Status400BadRequest,
                "Invalid tool reference.",
                "The requested tool was not found.");
        }

        if (tool.Status == ToolStatus.Retired)
        {
            return ServiceResult<MaintenanceRecord>.Fail(
                StatusCodes.Status400BadRequest,
                "Tool is retired.",
                "Retired tools cannot be moved into maintenance.");
        }

        if (tool.Status == ToolStatus.CheckedOut)
        {
            return ServiceResult<MaintenanceRecord>.Fail(
                StatusCodes.Status400BadRequest,
                "Tool is checked out.",
                "Check in the tool before starting maintenance.");
        }

        var record = new MaintenanceRecord
        {
            ToolId = dto.ToolId,
            Date = dto.Date,
            Description = InputNormalizer.NormalizeName(dto.Description),
            PerformedBy = InputNormalizer.NormalizeOptionalText(dto.PerformedBy),
            Cost = dto.Cost,
            NextMaintenanceDate = dto.NextMaintenanceDate
        };

        tool.Status = ToolStatus.UnderMaintenance;
        uow.Tools.Update(tool);
        await uow.MaintenanceRecords.AddAsync(record);
        await uow.SaveChangesAsync(cancellationToken);
        return ServiceResult<MaintenanceRecord>.Success(record);
    }

    public async Task<ServiceResult<MaintenanceRecord>> CompleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var record = await uow.MaintenanceRecords.GetByIdAsync(id, cancellationToken);
        if (record is null)
        {
            return ServiceResult<MaintenanceRecord>.Fail(StatusCodes.Status404NotFound, "Maintenance record not found.", "The requested maintenance record does not exist.");
        }

        var tool = await uow.Tools.GetByIdAsync(record.ToolId, cancellationToken);
        if (tool is not null && tool.Status == ToolStatus.UnderMaintenance)
        {
            tool.Status = ToolStatus.Available;
            uow.Tools.Update(tool);
        }

        await uow.SaveChangesAsync(cancellationToken);
        return ServiceResult<MaintenanceRecord>.Success(record);
    }

    public async Task<ServiceResult<MaintenanceRecord>> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var record = await uow.MaintenanceRecords.GetByIdAsync(id, cancellationToken);
        if (record is null)
        {
            return ServiceResult<MaintenanceRecord>.Fail(StatusCodes.Status404NotFound, "Maintenance record not found.", "The requested maintenance record does not exist.");
        }

        var tool = await uow.Tools.GetByIdAsync(record.ToolId, cancellationToken);
        if (tool is not null && tool.Status == ToolStatus.UnderMaintenance)
        {
            var hasOtherRecords = await uow.HasOtherMaintenanceRecordsForToolAsync(tool.Id, record.Id, cancellationToken);
            if (!hasOtherRecords)
            {
                tool.Status = ToolStatus.Available;
                uow.Tools.Update(tool);
            }
        }

        uow.MaintenanceRecords.Remove(record);
        await uow.SaveChangesAsync(cancellationToken);
        return ServiceResult<MaintenanceRecord>.Success(record);
    }
}
