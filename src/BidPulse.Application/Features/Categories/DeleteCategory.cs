using BidPulse.Application.Common.Interfaces;
using FluentValidation;
using Immediate.Handlers.Shared;
using ErrorOr;

namespace BidPulse.Application.Features.Categories;

[Handler]
public static partial class DeleteCategory
{
    public sealed record Command(Guid Id);

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id cannot be empty");
            
        }
    }

    private static async ValueTask<ErrorOr<Deleted>> HandleAsync(
        Command request,
        IAppDbContext context,
        IValidator<Command> validator,
        CancellationToken ct = default)
    {
        var validationResult = await validator.ValidateAsync(request, ct);

        if (!validationResult.IsValid)
            return validationResult.Errors
                .Select(e => Error.Validation(e.PropertyName, e.ErrorMessage))
                .ToList();
        
        var category = await context.Categories.FindAsync([request.Id], ct);

        if (category is null)
            return Error.NotFound("Category.NotFound", "Category not found.");

        context.Categories.Remove(category);
        
        await context.SaveChangesAsync(ct);

        return Result.Deleted;
    }
}