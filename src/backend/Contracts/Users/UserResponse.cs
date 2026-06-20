namespace BidPulse.Contracts.Users;

public sealed record UserResponse(
    Guid Id,
    string NickName,
    string Email,
    decimal Balance,
    string Role,
    DateTimeOffset CreatedAt);