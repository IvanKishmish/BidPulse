namespace BidPulse.Contracts.Wallet;

public sealed record WalletTransactionResponse(
    Guid Id,
    Guid UserId,
    decimal Amount,
    string Type,
    Guid? LotId,
    DateTimeOffset CreatedAt);