using Microsoft.AspNetCore.Identity;
using ToolInventory.API.Common;
using ToolInventory.Core.Entities;
using ToolInventory.Core.Interfaces;
using ToolInventory.Shared.DTOs;

namespace ToolInventory.API.Services;

public class CheckoutService(IUnitOfWork uow, UserManager<IdentityUser> userManager) : ICheckoutService
{
    public async Task<ServiceResult<Checkout>> CreateAsync(CreateCheckoutDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.ExpectedReturnDate.HasValue && dto.ExpectedReturnDate.Value <= DateTime.UtcNow)
        {
            return ServiceResult<Checkout>.Fail(
                StatusCodes.Status400BadRequest,
                "Invalid expected return date.",
                "Expected return date must be in the future.");
        }

        var tool = await uow.Tools.GetByIdAsync(dto.ToolId, cancellationToken);
        if (tool is null)
        {
            return ServiceResult<Checkout>.Fail(
                StatusCodes.Status400BadRequest,
                "Invalid tool reference.",
                "The requested tool was not found.");
        }

        if (tool.Status == ToolStatus.Retired)
        {
            return ServiceResult<Checkout>.Fail(
                StatusCodes.Status400BadRequest,
                "Tool is retired.",
                "Retired tools cannot be checked out.");
        }

        if (tool.Status != ToolStatus.Available)
        {
            return ServiceResult<Checkout>.Fail(
                StatusCodes.Status400BadRequest,
                "Tool unavailable.",
                "The tool is not currently available for checkout.");
        }

        var userId = InputNormalizer.NormalizeName(dto.UserId);
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return ServiceResult<Checkout>.Fail(
                StatusCodes.Status400BadRequest,
                "Invalid user reference.",
                "The requesting user was not found.");
        }

        var checkout = new Checkout
        {
            ToolId = dto.ToolId,
            UserId = userId,
            CheckoutDate = DateTime.UtcNow,
            ExpectedReturnDate = dto.ExpectedReturnDate,
            Notes = InputNormalizer.NormalizeOptionalText(dto.Notes)
        };

        tool.Status = ToolStatus.CheckedOut;
        uow.Tools.Update(tool);
        await uow.Checkouts.AddAsync(checkout);
        await uow.SaveChangesAsync(cancellationToken);

        return ServiceResult<Checkout>.Success(checkout);
    }

    public async Task<ServiceResult<Checkout>> CheckInAsync(int checkoutId, CancellationToken cancellationToken = default)
    {
        var checkout = await uow.Checkouts.GetByIdAsync(checkoutId, cancellationToken);
        if (checkout is null)
        {
            return ServiceResult<Checkout>.Fail(StatusCodes.Status404NotFound, "Checkout not found.", "The requested checkout does not exist.");
        }

        return await CompleteCheckInAsync(checkout, cancellationToken);
    }

    public async Task<ServiceResult<Checkout>> CheckInByToolIdAsync(int toolId, CancellationToken cancellationToken = default)
    {
        var checkout = await uow.GetLatestActiveCheckoutByToolIdAsync(toolId, cancellationToken);
        if (checkout is null)
        {
            return ServiceResult<Checkout>.Fail(
                StatusCodes.Status404NotFound,
                "No active checkout found.",
                "There is no active checkout for this tool.");
        }

        return await CompleteCheckInAsync(checkout, cancellationToken);
    }

    private async Task<ServiceResult<Checkout>> CompleteCheckInAsync(Checkout checkout, CancellationToken cancellationToken)
    {
        if (checkout.ActualReturnDate.HasValue)
        {
            return ServiceResult<Checkout>.Fail(
                StatusCodes.Status400BadRequest,
                "Checkout already completed.",
                "This checkout has already been checked in.");
        }

        var now = DateTime.UtcNow;
        if (now < checkout.CheckoutDate)
        {
            return ServiceResult<Checkout>.Fail(
                StatusCodes.Status400BadRequest,
                "Invalid check-in timestamp.",
                "Check-in date cannot be before checkout date.");
        }

        checkout.ActualReturnDate = now;
        var tool = await uow.Tools.GetByIdAsync(checkout.ToolId, cancellationToken);
        if (tool is not null)
        {
            tool.Status = ToolStatus.Available;
            uow.Tools.Update(tool);
        }

        uow.Checkouts.Update(checkout);
        await uow.SaveChangesAsync(cancellationToken);
        return ServiceResult<Checkout>.Success(checkout);
    }
}
