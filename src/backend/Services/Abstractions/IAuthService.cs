using BidPulse.Contracts.Auth;
using ErrorOr;

namespace BidPulse.Services.Abstractions;

public interface IAuthService
{
    Task<ErrorOr<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<ErrorOr<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default);
}