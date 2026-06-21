using BidPulse.Domain.Entities.Models;
using ErrorOr;

namespace BidPulse.Domain.Entities;

public sealed class Bid : Entity
{
    public Guid LotId { get; private set;}
    public Guid BidderId { get; private set; }
    public decimal Amount { get; private set; }

    //nav props
    public AuctionLot Lot { get; private set; } = null!;
    public User Bidder { get; private set; } = null!;
    
    private Bid()
    { } //ef

    private Bid(Guid id, BidCreationParams args) : base(id)
    {
        LotId = args.LotId;
        BidderId = args.BidderId;
        Amount = args.Amount;
    }

    public static ErrorOr<Bid> Create(BidCreationParams args)
    {
        var validationResult = ValidateInvariants(args.Amount);

        if (validationResult.IsError)
            return validationResult.Errors;

        return new Bid(Guid.CreateVersion7(), args);
    }

    public ErrorOr<Updated> UpdateAmount(decimal newAmount)
    {
        var validationResult = ValidateInvariants(newAmount);
        
        if (validationResult.IsError)
            return validationResult.Errors;

        Amount = newAmount;

        return Result.Updated;
    }

    private static ErrorOr<Success> ValidateInvariants(decimal amount)
    {
        var errors = new List<Error>();

        if (amount <= 0)
            errors.Add(Error.Validation("Bid.InvalidAmount", "Bid amount must be greater than zero."));

        return errors.Count > 0 ? errors : Result.Success;
    }
}