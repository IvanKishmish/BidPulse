using BidPulse.Domain.Enums;

namespace BidPulse.Domain.Entities.Models;

public sealed record WalletTransactionCreationParams(
    Guid UserId,
    decimal Amount,
    TransactionType Type,
    Guid? LotId = null 
);