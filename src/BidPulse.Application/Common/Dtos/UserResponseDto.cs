using BidPulse.Domain.Enums;

namespace BidPulse.Application.Common.Dtos;

public sealed record UserResponseDto(Guid Id, string NickName, string Email, decimal Balance, Role Role);