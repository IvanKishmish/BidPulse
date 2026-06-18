namespace BidPulse.Contracts.Users;

public sealed record CreateUserRequest(
    string NickName,
    string Email,
    string Password);