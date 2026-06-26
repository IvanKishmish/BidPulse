using BidPulse.Application.Common.Interfaces;
using BidPulse.Domain.Entities;
using FluentValidation;
using Immediate.Handlers.Shared;
using ErrorOr;

namespace BidPulse.Application.Features.Categories;

[Handler]
public static partial class CreateCategory
{
    public sealed record Command(string Name, string Description);

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100)
                .WithMessage("Name must not exceed 100 characters.");

            RuleFor(x => x.Description)
                .NotEmpty()
                .MaximumLength(500)
                .WithMessage("Description must not exceed 500 characters.");
        }
    }

    private static async ValueTask<ErrorOr<Guid>> HandleAsync(
        Command request,
        IAppDbContext db,
        IValidator<Command> validator,
        CancellationToken ct = default)
    {
        var validationResult = await validator.ValidateAsync(request, ct);

        if (!validationResult.IsValid)
            return validationResult.Errors
                .Select(e => Error.Validation(e.PropertyName, e.ErrorMessage))
                .ToList();

        var categoryResult = Category.Create(request.Name, request.Description);
        if (categoryResult.IsError)
            return categoryResult.Errors;
        
        await db.Categories.AddAsync(categoryResult.Value, ct);
        await db.SaveChangesAsync(ct);

        return categoryResult.Value.Id;
    }
}