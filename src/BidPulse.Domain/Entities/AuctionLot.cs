using BidPulse.Domain.Entities.Models;
using BidPulse.Domain.Enums;
using ErrorOr;

namespace BidPulse.Domain.Entities;

public sealed class AuctionLot : Entity
{
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal StartPrice { get; private set; }
    public decimal CurrentPrice { get; private set;}
    public decimal MinBidStep { get; private set; }
    public DateTimeOffset StartsAt { get; private set; }
    public DateTimeOffset EndsAt { get; private set; }
    public LotStatus Status { get; private set; }
    public Guid CategoryId { get; private set; }
    public Guid CreatorId { get; private set; }

    //navigation properties
    public Category Category { get; private set; } = null!;
    public User User { get; private set; } = null!;

    private AuctionLot()
    { }//ef

    private AuctionLot(Guid id, AuctionLotCreationParams args) : base(id)
    {
        Title = args.Title;
        Description = args.Description;
        StartPrice = args.StartPrice;
        MinBidStep = args.MinBidStep;
        StartsAt = args.StartsAt;
        EndsAt = args.EndsAt;
        CategoryId = args.CategoryId;
        CreatorId = args.CreatorId;

        CurrentPrice = args.StartPrice; // Current price is equal to start price at the start
        Status = LotStatus.Active;
    }

    public static ErrorOr<AuctionLot> Create(AuctionLotCreationParams args)
    {
        var validationResult = ValidateInvariants(args);

        if (validationResult.IsError)
            return validationResult.Errors;

        return new AuctionLot(Guid.CreateVersion7(), args);
    }

    public ErrorOr<Updated> UpdateAuctionLot(AuctionLotCreationParams args)
    {
        var validationResult = ValidateInvariants(args);
        
        if (validationResult.IsError)
            return validationResult.Errors;
        
        if (Status is LotStatus.Completed or LotStatus.Cancelled)
        {
            return Error.Validation(
                "AuctionLot.InvalidStatusForUpdate", 
                "Cannot update an auction lot that is already completed or cancelled.");
        }
        
        // If the auction has already started, changing the start time makes no sense
        if (StartsAt <= DateTimeOffset.UtcNow && args.StartsAt != StartsAt)
        {
            return Error.Validation(
                "AuctionLot.AlreadyStarted", 
                "Cannot change the start time of an auction that has already begun.");
        }
        
        // Critical Auction Rule: If active bids exist, pricing logic cannot be altered
        if (CurrentPrice > StartPrice && (args.StartPrice != StartPrice || args.MinBidStep != MinBidStep))
        {
            return Error.Validation(
                "AuctionLot.BidsAlreadyPlaced", 
                "Cannot change the start price or minimum bid step after bids have been placed.");
        }
        
        Title = args.Title;
        Description = args.Description;
        StartPrice = args.StartPrice;
        MinBidStep = args.MinBidStep;
        StartsAt = args.StartsAt;
        EndsAt = args.EndsAt;
        CategoryId = args.CategoryId;

        return Result.Updated;
    }

    /// <summary>
    /// Advances the current price to the winning bid amount.
    /// Called exclusively by BidService inside a transaction after all validations pass.
    /// </summary>
    public ErrorOr<Updated> SetCurrentPrice(decimal newPrice)
    {
        if (newPrice <= CurrentPrice)
            return Error.Validation(
                "AuctionLot.InvalidPrice",
                "New price must be strictly greater than the current price.");
 
        CurrentPrice = newPrice;
        return Result.Updated;
    }
 
    /// <summary>
    /// Cancels the lot. The caller is responsible for refunding the current top bidder.
    /// </summary>
    public ErrorOr<Updated> Cancel()
    {
        if (Status == LotStatus.Completed)
            return Error.Validation(
                "AuctionLot.AlreadyCompleted",
                "Cannot cancel an auction that has already been completed.");
 
        if (Status == LotStatus.Cancelled)
            return Error.Validation(
                "AuctionLot.AlreadyCancelled",
                "This auction lot has already been cancelled.");
 
        Status = LotStatus.Cancelled;
        return Result.Updated;
    }
 
    /// <summary>
    /// Marks the lot as completed (e.g., called by a background job when EndsAt is reached).
    /// </summary>
    public ErrorOr<Updated> Complete()
    {
        if (Status != LotStatus.Active)
            return Error.Validation(
                "AuctionLot.NotActive",
                "Only active auctions can be marked as completed.");
 
        Status = LotStatus.Completed;
        return Result.Updated;
    }
    
    private static ErrorOr<Success> ValidateInvariants(AuctionLotCreationParams args)
    {
        var errors = new List<Error>();

        if (string.IsNullOrWhiteSpace(args.Title))
            errors.Add(Error.Validation("AuctionLot.TitleRequired", "AuctionLot title cannot be empty."));
            
        if (string.IsNullOrWhiteSpace(args.Description))
            errors.Add(Error.Validation("AuctionLot.DescriptionRequired", "AuctionLot description cannot be empty."));

        if (args.StartPrice < 0)
            errors.Add(Error.Validation("AuctionLot.InvalidStartPrice", "Start price cannot be negative."));

        if (args.MinBidStep <= 0)
            errors.Add(Error.Validation("AuctionLot.InvalidMinBidStep", "Minimum bid step must be greater than zero."));

        if (args.StartsAt >= args.EndsAt)
            errors.Add(Error.Validation("AuctionLot.InvalidDates", "Auction start time must be before the end time."));

        if (args.EndsAt <= DateTimeOffset.UtcNow)
            errors.Add(Error.Validation("AuctionLot.AuctionEnded", "Cannot create an auction that has already ended in the past."));

        return errors.Count > 0 ? errors : Result.Success;
    }
}