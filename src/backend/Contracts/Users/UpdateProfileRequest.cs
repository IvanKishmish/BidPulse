namespace BidPulse.Contracts.Users;

public sealed record UpdateProfileRequest(
    string NickName,
    string Email);