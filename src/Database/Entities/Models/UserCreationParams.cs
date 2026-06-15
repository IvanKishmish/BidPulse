using BidPulse.Database.Entities.Enums;

namespace BidPulse.Database.Entities.Models;

public sealed record UserCreationParams(
    string NickName,
    string Email,
    string PasswordHash,
    decimal Balance,
    Role Role);