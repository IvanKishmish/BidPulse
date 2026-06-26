using BidPulse.Application.Common.Interfaces;
using ErrorOr;
using FluentValidation;
using Immediate.Handlers.Shared;
using Microsoft.EntityFrameworkCore;

namespace BidPulse.Application.Features.Categories;

[Handler]
public static partial class GetCategoryById
{
    public sealed record Query(Guid Id);
    
    public sealed class Validator : AbstractValidator<Query> 
    {
        public Validator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id cannot be empty.");
        }
    }

    private static async ValueTask<ErrorOr<CategoryResponseDto>> HandleAsync(
        Query request,
        IAppDbContext context,
        IValidator<Query> validator,
        CancellationToken ct = default)
    {
        var validationResult = await validator.ValidateAsync(request, ct);

        if (!validationResult.IsValid)
            return validationResult.Errors
                .Select(e => Error.Validation(e.PropertyName, e.ErrorMessage))
                .ToList();
        
        var category = await context.Categories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Id, ct);

        if (category is null)
            return Error.NotFound("Category.NotFound", "Category not found.");
        
        return new CategoryResponseDto(category.Id, category.Name, category.Description);
    }
}