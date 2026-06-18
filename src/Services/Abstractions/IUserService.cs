using BidPulse.Contracts.Users;
using ErrorOr;

namespace BidPulse.Services.Abstractions;

public interface IUserService
{
    Task<ErrorOr<List<UserResponse>>> GetAllAsync(CancellationToken ct = default);
    Task<ErrorOr<UserResponse>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ErrorOr<UserResponse>> CreateAsync(CreateUserRequest request, CancellationToken ct = default);
    Task<ErrorOr<UserResponse>> UpdateProfileAsync(Guid id, UpdateProfileRequest request, CancellationToken ct = default);
    Task<ErrorOr<UserResponse>> ChangeRoleAsync(Guid id, ChangeRoleRequest request, CancellationToken ct = default);
    Task<ErrorOr<Deleted>> DeleteAsync(Guid id, CancellationToken ct = default);
}