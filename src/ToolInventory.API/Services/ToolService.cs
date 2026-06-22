using ToolInventory.API.Common;
using ToolInventory.Core.Entities;
using ToolInventory.Core.Interfaces;
using ToolInventory.Shared.DTOs;

namespace ToolInventory.API.Services;

public class ToolService(IUnitOfWork uow) : IToolService
{
    public async Task<ServiceResult<Tool>> CreateAsync(CreateToolDto dto, CancellationToken cancellationToken = default)
    {
        var name = InputNormalizer.NormalizeName(dto.Name);
        var requestedBarcode = InputNormalizer.NormalizeBarcode(dto.Barcode);
        var barcode = requestedBarcode ?? GenerateBarcode();

        var existingBarcode = (await uow.Tools.FindAsync(t => t.Barcode == barcode, cancellationToken: cancellationToken)).FirstOrDefault();
        if (existingBarcode is not null)
        {
            if (requestedBarcode is not null)
            {
                return ServiceResult<Tool>.Fail(
                    StatusCodes.Status409Conflict,
                    "Duplicate barcode.",
                    "Another tool already uses this barcode.");
            }

            do
            {
                barcode = GenerateBarcode();
                existingBarcode = (await uow.Tools.FindAsync(t => t.Barcode == barcode, cancellationToken: cancellationToken)).FirstOrDefault();
            } while (existingBarcode is not null);
        }

        if (dto.CategoryId.HasValue)
        {
            var category = await uow.Categories.GetByIdAsync(dto.CategoryId.Value, cancellationToken);
            if (category is null)
            {
                return ServiceResult<Tool>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Invalid category reference.",
                    "The selected category does not exist.");
            }
        }

        var tool = new Tool
        {
            Name = name,
            Make = InputNormalizer.NormalizeOptionalText(dto.Make),
            Model = InputNormalizer.NormalizeOptionalText(dto.Model),
            SerialNumber = InputNormalizer.NormalizeOptionalText(dto.SerialNumber),
            Owner = InputNormalizer.NormalizeOptionalText(dto.Owner),
            Description = InputNormalizer.NormalizeOptionalText(dto.Description),
            Barcode = barcode,
            Location = InputNormalizer.NormalizeOptionalText(dto.Location),
            Systainer = InputNormalizer.NormalizeOptionalText(dto.Systainer),
            ImageUrl = InputNormalizer.NormalizeOptionalText(dto.ImageUrl),
            CategoryId = dto.CategoryId
        };

        await uow.Tools.AddAsync(tool);
        await uow.SaveChangesAsync(cancellationToken);
        return ServiceResult<Tool>.Success(tool);
    }

    public async Task<ServiceResult<Tool>> UpdateAsync(int id, UpdateToolDto dto, CancellationToken cancellationToken = default)
    {
        var tool = await uow.Tools.GetByIdAsync(id, cancellationToken);
        if (tool is null)
        {
            return ServiceResult<Tool>.Fail(StatusCodes.Status404NotFound, "Tool not found.", "The requested tool does not exist.");
        }

        if (dto.CategoryId.HasValue)
        {
            var category = await uow.Categories.GetByIdAsync(dto.CategoryId.Value, cancellationToken);
            if (category is null)
            {
                return ServiceResult<Tool>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Invalid category reference.",
                    "The selected category does not exist.");
            }
        }

        var parsedStatus = ParseStatus(dto.Status);
        if (!IsValidStatusTransition(tool.Status, parsedStatus))
        {
            return ServiceResult<Tool>.Fail(
                StatusCodes.Status400BadRequest,
                "Invalid status transition.",
                $"Cannot transition from '{tool.Status}' to '{parsedStatus}'.");
        }

        tool.Name = InputNormalizer.NormalizeName(dto.Name);
        tool.Make = InputNormalizer.NormalizeOptionalText(dto.Make);
        tool.Model = InputNormalizer.NormalizeOptionalText(dto.Model);
        tool.SerialNumber = InputNormalizer.NormalizeOptionalText(dto.SerialNumber);
        tool.Owner = InputNormalizer.NormalizeOptionalText(dto.Owner);
        tool.Description = InputNormalizer.NormalizeOptionalText(dto.Description);
        tool.Location = InputNormalizer.NormalizeOptionalText(dto.Location);
        tool.Systainer = InputNormalizer.NormalizeOptionalText(dto.Systainer);
        tool.ImageUrl = InputNormalizer.NormalizeOptionalText(dto.ImageUrl);
        tool.CategoryId = dto.CategoryId;
        tool.Status = parsedStatus;
        uow.Tools.Update(tool);
        await uow.SaveChangesAsync(cancellationToken);
        return ServiceResult<Tool>.Success(tool);
    }

    private static string GenerateBarcode() => Guid.NewGuid().ToString().ToUpperInvariant();

    private static ToolStatus ParseStatus(ToolStatusValue status)
        => status switch
        {
            ToolStatusValue.Available => ToolStatus.Available,
            ToolStatusValue.CheckedOut => ToolStatus.CheckedOut,
            ToolStatusValue.UnderMaintenance => ToolStatus.UnderMaintenance,
            ToolStatusValue.Retired => ToolStatus.Retired,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unsupported status value.")
        };

    private static bool IsValidStatusTransition(ToolStatus current, ToolStatus next)
    {
        if (current == next)
        {
            return true;
        }

        return (current, next) switch
        {
            (ToolStatus.Available, ToolStatus.CheckedOut) => true,
            (ToolStatus.Available, ToolStatus.UnderMaintenance) => true,
            (ToolStatus.Available, ToolStatus.Retired) => true,
            (ToolStatus.CheckedOut, ToolStatus.Available) => true,
            (ToolStatus.UnderMaintenance, ToolStatus.Available) => true,
            (ToolStatus.UnderMaintenance, ToolStatus.Retired) => true,
            _ => false
        };
    }
}
