using BidPulse.Contracts.Categories;
using ErrorOr;

namespace BidPulse.Services.Abstractions;

public interface ICategoryService
{
    Task<ErrorOr<List<CategoryResponse>>> GetAllAsync(CancellationToken ct = default);
    Task<ErrorOr<CategoryResponse>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ErrorOr<CategoryResponse>> CreateAsync(CreateCategoryRequest request, CancellationToken ct = default);
    Task<ErrorOr<CategoryResponse>> UpdateAsync(Guid id, CreateCategoryRequest request, CancellationToken ct = default);
    Task<ErrorOr<Deleted>> DeleteAsync(Guid id, CancellationToken ct = default);
}