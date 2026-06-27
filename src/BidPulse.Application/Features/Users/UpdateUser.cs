using BidPulse.Application.Common.Interfaces;
using BidPulse.Domain.Enums;
using FluentValidation;
using Immediate.Handlers.Shared;
using ErrorOr;

namespace BidPulse.Application.Features.Users;

[Handler]
public static partial class UpdateUser
{
    public sealed record Command(Guid Id, string NickName, string Email);

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required");
            RuleFor(x => x.NickName).NotEmpty().MinimumLength(6).WithMessage("Nickname must be at least 6 characters");
            RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Email must be a valid email address");
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
            
        if(user is null)
            return Error.NotFound("User.NotFound", "User not found");
            
        var result = user.UpdateProfile(request.NickName, request.Email);

        if (result.IsError)
            return result.Errors;
            
        await context.SaveChangesAsync(ct);

        return Result.Updated;
    }
}