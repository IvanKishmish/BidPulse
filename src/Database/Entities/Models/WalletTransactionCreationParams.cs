using BidPulse.Database.Entities.Enums;

namespace BidPulse.Database.Entities.Models;

public sealed record WalletTransactionCreationParams(
    Guid UserId,
    decimal Amount,
    TransactionType Type,
    Guid? LotId = null 
);