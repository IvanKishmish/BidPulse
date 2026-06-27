using BidPulse.Application.Common.Interfaces;
using BidPulse.Domain.Enums;
using ErrorOr;
using FluentValidation;
using Immediate.Handlers.Shared;

namespace BidPulse.Application.Features.Users;

[Handler]
public static partial class ChangeUserRole
{
    public sealed record Command(Guid Id, Role Role);
    
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required");
            RuleFor(x => x.Role).IsInEnum().WithMessage("Invalid role.");
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

        var user = await context.Users.FindAsync([request.Id], ct);
        if (user is null)
            return Error.NotFound("User.NotFound", "User not found.");

        var result = user.ChangeRole(request.Role);
        if (result.IsError)
            return result.Errors;

        await context.SaveChangesAsync(ct);
        return Result.Updated;
    }
}