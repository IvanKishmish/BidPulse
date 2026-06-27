using BidPulse.Application.Common.Interfaces;
using BidPulse.Domain.Entities;
using BidPulse.Domain.Entities.Models;
using FluentValidation;
using ErrorOr;
using Immediate.Handlers.Shared;

namespace BidPulse.Application.Features.AuctionLots;

[Handler]
public static partial class CreateAuctionLot
{
    public sealed record Command(
        string Title,
        string Description,
        decimal StartPrice,
        decimal MinBidStep,
        DateTimeOffset StartsAt,
        DateTimeOffset EndsAt,
        Guid CategoryId,
        Guid CreatorId);

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Description).NotEmpty().MaximumLength(1000);
            RuleFor(x => x.StartPrice).GreaterThanOrEqualTo(0);
            RuleFor(x => x.MinBidStep).GreaterThan(0);
            RuleFor(x => x.StartsAt).LessThan(x => x.EndsAt).WithMessage("Start time must be before end time.");
            RuleFor(x => x.EndsAt).GreaterThan(DateTimeOffset.UtcNow).WithMessage("End time must be in the future.");
            RuleFor(x => x.CategoryId).NotEmpty();
            RuleFor(x => x.CreatorId).NotEmpty();
        }
    }

    private static async ValueTask<ErrorOr<Guid>> HandleAsync(
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

        var lotResult = AuctionLot.Create(new AuctionLotCreationParams(
            request.Title,
            request.Description,
            request.StartPrice,
            request.MinBidStep,
            request.StartsAt,
            request.EndsAt,
            request.CategoryId,
            request.CreatorId));

        if (lotResult.IsError)
            return lotResult.Errors;

        await context.Lots.AddAsync(lotResult.Value, ct);
        await context.SaveChangesAsync(ct);
        return lotResult.Value.Id;
    }
}