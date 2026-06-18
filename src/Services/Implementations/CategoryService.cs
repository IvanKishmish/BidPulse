using BidPulse.Contracts.Categories;
using BidPulse.Database;
using BidPulse.Database.Entities;
using BidPulse.Services.Abstractions;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace BidPulse.Services.Implementations;

public sealed class CategoryService(AppDbContext context) : ICategoryService
{
    public async Task<ErrorOr<List<CategoryResponse>>> GetAllAsync(CancellationToken ct = default)
    {
        var categories = await context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
 
        return categories.Select(ToResponse).ToList();
    }
 
    public async Task<ErrorOr<CategoryResponse>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var category = await context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct);
 
        if (category is null)
            return Error.NotFound("Category.NotFound", $"Category with id '{id}' was not found.");
 
        return ToResponse(category);
    }
 
    public async Task<ErrorOr<CategoryResponse>> CreateAsync(CreateCategoryRequest request, CancellationToken ct = default)
    {
        var nameExists = await context.Categories.AnyAsync(c => c.Name == request.Name, ct);
        if (nameExists)
            return Error.Conflict("Category.NameConflict", $"A category named '{request.Name}' already exists.");
 
        var createResult = Category.Create(request.Name, request.Description);
        if (createResult.IsError)
            return createResult.Errors;
 
        context.Categories.Add(createResult.Value);
        await context.SaveChangesAsync(ct);
 
        return ToResponse(createResult.Value);
    }
 
    public async Task<ErrorOr<CategoryResponse>> UpdateAsync(
        Guid id, CreateCategoryRequest request, CancellationToken ct = default)
    {
        var category = await context.Categories.FindAsync([id], ct);
        if (category is null)
            return Error.NotFound("Category.NotFound", $"Category with id '{id}' was not found.");
 
        var nameConflict = await context.Categories
            .AnyAsync(c => c.Name == request.Name && c.Id != id, ct);
 
        if (nameConflict)
            return Error.Conflict("Category.NameConflict", $"A category named '{request.Name}' already exists.");
 
        var updateResult = category.UpdateCategory(request.Name, request.Description);
        if (updateResult.IsError)
            return updateResult.Errors;
 
        await context.SaveChangesAsync(ct);
        return ToResponse(category);
    }
 
    public async Task<ErrorOr<Deleted>> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var category = await context.Categories.FindAsync([id], ct);
        if (category is null)
            return Error.NotFound("Category.NotFound", $"Category with id '{id}' was not found.");
 
        var hasLots = await context.Lots.AnyAsync(l => l.CategoryId == id, ct);
        if (hasLots)
            return Error.Conflict(
                "Category.HasActiveLots",
                "Cannot delete a category that still contains auction lots.");
 
        context.Categories.Remove(category);
        await context.SaveChangesAsync(ct);
        return Result.Deleted;
    }
 
    private static CategoryResponse ToResponse(Category c) => new(c.Id, c.Name, c.Description);
}