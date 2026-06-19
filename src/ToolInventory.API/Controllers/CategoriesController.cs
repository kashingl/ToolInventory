using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToolInventory.API.Common;
using ToolInventory.Core.Entities;
using ToolInventory.Core.Interfaces;
using ToolInventory.Shared.DTOs;

namespace ToolInventory.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CategoriesController(IUnitOfWork uow) : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<CategoryDto>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);
        return (await uow.Categories.GetPagedAsync(page, pageSize, cancellationToken: cancellationToken)).Select(c => new CategoryDto { Id = c.Id, Name = c.Name, Description = c.Description });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryDto>> GetById(int id, CancellationToken cancellationToken = default)
    {
        var cat = await uow.Categories.GetByIdAsync(id, cancellationToken);
        return cat is null ? NotFound() : Ok(new CategoryDto { Id = cat.Id, Name = cat.Name, Description = cat.Description });
    }

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create(CreateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var name = InputNormalizer.NormalizeName(dto.Name);
        var existing = (await uow.Categories.FindAsync(c => c.Name == name, cancellationToken: cancellationToken)).FirstOrDefault();
        if (existing is not null)
        {
            return this.ConflictProblem("Duplicate category name.", "A category with this name already exists.");
        }

        var cat = new Category { Name = name, Description = InputNormalizer.NormalizeOptionalText(dto.Description) };
        await uow.Categories.AddAsync(cat);
        await uow.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = cat.Id }, new CategoryDto { Id = cat.Id, Name = cat.Name, Description = cat.Description });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var cat = await uow.Categories.GetByIdAsync(id, cancellationToken);
        if (cat is null) return NotFound();

        var name = InputNormalizer.NormalizeName(dto.Name);
        var existing = (await uow.Categories.FindAsync(c => c.Name == name && c.Id != id, cancellationToken: cancellationToken)).FirstOrDefault();
        if (existing is not null)
        {
            return this.ConflictProblem("Duplicate category name.", "A category with this name already exists.");
        }

        cat.Name = name;
        cat.Description = InputNormalizer.NormalizeOptionalText(dto.Description);
        uow.Categories.Update(cat);
        await uow.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        var cat = await uow.Categories.GetByIdAsync(id, cancellationToken);
        if (cat is null) return NotFound();

        var hasTools = (await uow.Tools.FindAsync(t => t.CategoryId == id, cancellationToken: cancellationToken)).Any();
        if (hasTools)
        {
            return this.ConflictProblem("Category in use.", "The category cannot be deleted because one or more tools still reference it.");
        }

        uow.Categories.Remove(cat);
        await uow.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
