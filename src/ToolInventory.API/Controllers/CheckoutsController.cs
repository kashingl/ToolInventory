using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToolInventory.API.Services;
using ToolInventory.Core.Entities;
using ToolInventory.Core.Interfaces;
using ToolInventory.Shared.DTOs;

namespace ToolInventory.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CheckoutsController(IUnitOfWork uow, UserManager<IdentityUser> userManager, ICheckoutService checkoutService) : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<CheckoutDto>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);
        var checkouts = (await uow.Checkouts.GetPagedAsync(page, pageSize, cancellationToken: cancellationToken, includes: c => c.Tool!)).ToList();
        var userIds = checkouts.Select(c => c.UserId).Distinct().ToList();
        var users = await userManager.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.UserName ?? u.Email ?? string.Empty, cancellationToken);

        return checkouts.Select(c => ToDto(c, users));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CheckoutDto>> GetById(int id, CancellationToken cancellationToken = default)
    {
        var c = (await uow.Checkouts.FindAsync(x => x.Id == id, cancellationToken: cancellationToken, includes: x => x.Tool!)).FirstOrDefault();
        if (c is null)
        {
            return NotFound();
        }

        var user = await userManager.FindByIdAsync(c.UserId);
        var users = new Dictionary<string, string>
        {
            [c.UserId] = user?.UserName ?? user?.Email ?? string.Empty
        };
        return Ok(ToDto(c, users));
    }

    [HttpPost]
    public async Task<ActionResult<CheckoutDto>> Create(CreateCheckoutDto dto, CancellationToken cancellationToken = default)
    {
        var result = await checkoutService.CreateAsync(dto, cancellationToken);
        if (!result.IsSuccess)
        {
            return StatusCode(result.Error!.StatusCode, new ProblemDetails
            {
                Status = result.Error.StatusCode,
                Title = result.Error.Title,
                Detail = result.Error.Detail
            });
        }

        var checkout = result.Value!;
        var user = await userManager.FindByIdAsync(checkout.UserId);

        var users = new Dictionary<string, string>
        {
            [checkout.UserId] = user?.UserName ?? user?.Email ?? string.Empty
        };
        return CreatedAtAction(nameof(GetById), new { id = checkout.Id }, ToDto(checkout, users));
    }

    [HttpPut("{id:int}/checkin")]
    public async Task<IActionResult> CheckIn(int id, CancellationToken cancellationToken = default)
    {
        var result = await checkoutService.CheckInAsync(id, cancellationToken);
        if (!result.IsSuccess)
        {
            return StatusCode(result.Error!.StatusCode, new ProblemDetails
            {
                Status = result.Error.StatusCode,
                Title = result.Error.Title,
                Detail = result.Error.Detail
            });
        }

        return NoContent();
    }

    [HttpPut("tool/{toolId:int}/checkin")]
    public async Task<IActionResult> CheckInByToolId(int toolId, CancellationToken cancellationToken = default)
    {
        var result = await checkoutService.CheckInByToolIdAsync(toolId, cancellationToken);
        if (!result.IsSuccess)
        {
            return StatusCode(result.Error!.StatusCode, new ProblemDetails
            {
                Status = result.Error.StatusCode,
                Title = result.Error.Title,
                Detail = result.Error.Detail
            });
        }

        return NoContent();
    }

    private static CheckoutDto ToDto(Checkout c, IReadOnlyDictionary<string, string> users) => new()
    {
        Id = c.Id,
        ToolId = c.ToolId,
        ToolName = c.Tool?.Name ?? string.Empty,
        UserId = c.UserId,
        UserName = users.TryGetValue(c.UserId, out var userName) ? userName : string.Empty,
        CheckoutDate = c.CheckoutDate,
        ExpectedReturnDate = c.ExpectedReturnDate,
        ActualReturnDate = c.ActualReturnDate,
        Notes = c.Notes
    };
}
