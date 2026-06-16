using BidPulse.Database.Entities.Enums;
using BidPulse.Database.Entities.Models;
using ErrorOr;

namespace BidPulse.Database.Entities;

public sealed class WalletTransaction : Entity
{
    public Guid UserId { get; private set; }
    public decimal Amount { get; private set; }
    public TransactionType Type { get; private set; }
    public Guid? LotId { get; private set; }

    // nav props
    public User User { get; private set; } = null!;
    public AuctionLot? Lot { get; private set; }

    private WalletTransaction()
    { } // ef

    private WalletTransaction(Guid id, WalletTransactionCreationParams args) : base(id)
    {
        UserId = args.UserId;
        Amount = args.Amount;
        Type = args.Type;
        LotId = args.LotId;
    }

    public static ErrorOr<WalletTransaction> Create(WalletTransactionCreationParams args)
    {
        var validationResult = ValidateInvariants(args);

        if (validationResult.IsError)
            return validationResult.Errors;

        return new WalletTransaction(Guid.CreateVersion7(), args);
    }

    private static ErrorOr<Success> ValidateInvariants(WalletTransactionCreationParams args)
    {
        var errors = new List<Error>();

        if (args.Amount <= 0)
            errors.Add(Error.Validation("Transaction.InvalidAmount", "Transaction amount must be greater than zero."));

        if (args.Type is TransactionType.HoldForBid or TransactionType.Refund or TransactionType.WinningPayment 
            && args.LotId == null)
        {
            errors.Add(Error.Validation("Transaction.LotRequired", $"LotId is required for {args.Type} transactions."));
        }

        if (args.Type is TransactionType.Deposit or TransactionType.Withdraw && args.LotId != null)
        {
            errors.Add(Error.Validation("Transaction.LotNotApplicable", $"LotId must be null for {args.Type} transactions."));
        }

        return errors.Count > 0 ? errors : Result.Success;
    }
}