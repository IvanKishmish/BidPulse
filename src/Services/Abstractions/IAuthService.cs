using BidPulse.Contracts.Auth;
using ErrorOr;

namespace BidPulse.Services.Abstractions;

public interface IAuthService
{
    Task<ErrorOr<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default);
}