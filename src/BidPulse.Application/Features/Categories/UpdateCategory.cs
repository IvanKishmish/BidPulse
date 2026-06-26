using BidPulse.Application.Common.Interfaces;
using FluentValidation;
using Immediate.Handlers.Shared;
using ErrorOr;

namespace BidPulse.Application.Features.Categories;

[Handler]
public static partial class UpdateCategory
{
    public sealed record Command(Guid Id, string Name, string Description);

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Id cannot be empty.");
            
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name cannot be empty.")
                .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");
            
            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description cannot be empty.")
                .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");
        }       
    }

    private static async ValueTask<ErrorOr<Updated>> HandleAsync(
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
        
        // var category = await context.Categories.FindAsync(request.Id, ct);
        
        //.NET9+ collection expression 
        var category = await context.Categories.FindAsync([request.Id], ct);

        if (category is null)
            return Error.NotFound("Category.NotFound", "Category not found.");
        
        var result = category.UpdateCategory(request.Name, request.Description);

        if (result.IsError)
            return result.Errors;
        
        await context.SaveChangesAsync(ct);
        return Result.Updated;
    }
}