using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToolInventory.API.Common;
using ToolInventory.API.Services;
using ToolInventory.Core.Entities;
using ToolInventory.Core.Interfaces;
using ToolInventory.Shared.DTOs;

namespace ToolInventory.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ToolsController(IUnitOfWork uow, IToolService toolService) : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<ToolDto>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 50, [FromQuery] string? status = null, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);

        var tools = !string.IsNullOrWhiteSpace(status) && Enum.TryParse<ToolStatus>(status, true, out var parsedStatus)
            ? await uow.Tools.GetPagedAsync(t => t.Status == parsedStatus, page, pageSize, cancellationToken: cancellationToken, includes: t => t.Category!)
            : await uow.Tools.GetPagedAsync(page, pageSize, cancellationToken: cancellationToken, includes: t => t.Category!);

        return tools.Select(ToDto);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ToolDto>> GetById(int id, CancellationToken cancellationToken = default)
    {
        var tool = (await uow.Tools.FindAsync(t => t.Id == id, cancellationToken: cancellationToken, includes: t => t.Category!)).FirstOrDefault();
        return tool is null ? NotFound() : Ok(ToDto(tool));
    }

    [HttpGet("barcode/{code}")]
    public async Task<ActionResult<ToolDto>> GetByBarcode(string code, CancellationToken cancellationToken = default)
    {
        var normalizedCode = InputNormalizer.NormalizeBarcode(code);
        var tools = await uow.Tools.FindAsync(t => t.Barcode == normalizedCode, cancellationToken: cancellationToken, includes: t => t.Category!);
        var tool = tools.FirstOrDefault();
        return tool is null ? NotFound() : Ok(ToDto(tool));
    }

    [HttpPost]
    public async Task<ActionResult<ToolDto>> Create(CreateToolDto dto, CancellationToken cancellationToken = default)
    {
        var result = await toolService.CreateAsync(dto, cancellationToken);
        if (!result.IsSuccess)
        {
            return StatusCode(result.Error!.StatusCode, new ProblemDetails
            {
                Status = result.Error.StatusCode,
                Title = result.Error.Title,
                Detail = result.Error.Detail
            });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, ToDto(result.Value));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateToolDto dto, CancellationToken cancellationToken = default)
    {
        var result = await toolService.UpdateAsync(id, dto, cancellationToken);
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
        var tool = await uow.Tools.GetByIdAsync(id, cancellationToken);
        if (tool is null) return NotFound();
        uow.Tools.Remove(tool);
        await uow.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static ToolDto ToDto(Tool t) => new()
    {
        Id = t.Id,
        Name = t.Name,
        Make = t.Make,
        Model = t.Model,
        SerialNumber = t.SerialNumber,
        Owner = t.Owner,
        Description = t.Description,
        Barcode = t.Barcode,
        Location = t.Location,
        Systainer = t.Systainer,
        Status = t.Status.ToString(),
        ImageUrl = t.ImageUrl,
        CategoryId = t.CategoryId,
        CategoryName = t.Category?.Name
    };

}
