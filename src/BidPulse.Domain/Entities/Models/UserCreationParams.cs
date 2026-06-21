using BidPulse.Domain.Enums;

namespace BidPulse.Domain.Entities.Models;

public sealed record UserCreationParams(
    string NickName,
    string Email,
    string PasswordHash,
    decimal Balance,
    Role Role);