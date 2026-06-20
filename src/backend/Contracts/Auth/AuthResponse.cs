namespace BidPulse.Contracts.Auth;

public sealed record AuthResponse(
    string Token,
    Guid UserId,
    string NickName,
    string Role);