using BidPulse.Application.Common.Dtos;
using BidPulse.Application.Common.Interfaces;
using FluentValidation;
using Immediate.Handlers.Shared;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace BidPulse.Application.Features.Users;

[Handler]
public static partial class GetUserById
{
    public sealed record Query(Guid Id);
    
    public sealed class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required");
        }
    }

    private static async ValueTask<ErrorOr<UserResponseDto>> HandleAsync(
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
        
        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.Id, ct);
        
        if(user is null)
            return Error.NotFound("User.NotFound", "User not found.");
        
        return new UserResponseDto(user.Id, user.NickName, user.Email, user.Balance, user.Role);
    }
}