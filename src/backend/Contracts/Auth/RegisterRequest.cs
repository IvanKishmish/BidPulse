namespace BidPulse.Contracts.Auth;

public sealed record RegisterRequest(
    string NickName,
    string Email,
    string Password);