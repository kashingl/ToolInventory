using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToolInventory.API.Services;
using ToolInventory.Core.Entities;
using ToolInventory.Core.Interfaces;
using ToolInventory.Shared.DTOs;

namespace ToolInventory.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MaintenanceController(IUnitOfWork uow, IMaintenanceService maintenanceService) : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<MaintenanceRecordDto>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);
        return (await uow.MaintenanceRecords.GetPagedAsync(page, pageSize, cancellationToken: cancellationToken, includes: m => m.Tool!)).Select(ToDto);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MaintenanceRecordDto>> GetById(int id, CancellationToken cancellationToken = default)
    {
        var r = (await uow.MaintenanceRecords.FindAsync(x => x.Id == id, cancellationToken: cancellationToken, includes: x => x.Tool!)).FirstOrDefault();
        return r is null ? NotFound() : Ok(ToDto(r));
    }

    [HttpGet("tool/{toolId:int}")]
    public async Task<IEnumerable<MaintenanceRecordDto>> GetByTool(int toolId, CancellationToken cancellationToken = default) =>
        (await uow.MaintenanceRecords.FindAsync(m => m.ToolId == toolId, cancellationToken: cancellationToken, includes: m => m.Tool!)).Select(ToDto);

    [HttpPost]
    public async Task<ActionResult<MaintenanceRecordDto>> Create(CreateMaintenanceRecordDto dto, CancellationToken cancellationToken = default)
    {
        var result = await maintenanceService.CreateAsync(dto, cancellationToken);
        if (!result.IsSuccess)
        {
            return StatusCode(result.Error!.StatusCode, new ProblemDetails
            {
                Status = result.Error.StatusCode,
                Title = result.Error.Title,
                Detail = result.Error.Detail
            });
        }

        var record = result.Value!;
        return CreatedAtAction(nameof(GetById), new { id = record.Id }, ToDto(record));
    }

    [HttpPut("{id:int}/complete")]
    public async Task<IActionResult> Complete(int id, CancellationToken cancellationToken = default)
    {
        var result = await maintenanceService.CompleteAsync(id, cancellationToken);
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

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        var result = await maintenanceService.DeleteAsync(id, cancellationToken);
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

    private static MaintenanceRecordDto ToDto(MaintenanceRecord r) => new()
    {
        Id = r.Id,
        ToolId = r.ToolId,
        ToolName = r.Tool?.Name ?? string.Empty,
        Date = r.Date,
        Description = r.Description,
        PerformedBy = r.PerformedBy,
        Cost = r.Cost,
        NextMaintenanceDate = r.NextMaintenanceDate
    };
}
