using BidPulse.Contracts.Users;
using BidPulse.Database;
using BidPulse.Database.Entities;
using BidPulse.Database.Entities.Enums;
using BidPulse.Database.Entities.Models;
using BidPulse.Services.Abstractions;
using ErrorOr;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BidPulse.Services.Implementations;

public sealed class UserService(
    AppDbContext context,
    IPasswordHasher<User> passwordHasher) : IUserService
{
    public async Task<ErrorOr<List<UserResponse>>> GetAllAsync(CancellationToken ct = default)
    {
        var users = await context.Users
            .AsNoTracking()
            .OrderBy(u => u.NickName)
            .ToListAsync(ct);
 
        return users.Select(ToResponse).ToList();
    }
 
    public async Task<ErrorOr<UserResponse>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, ct);
 
        if (user is null)
            return Error.NotFound("User.NotFound", $"User with id '{id}' was not found.");
 
        return ToResponse(user);
    }
 
    public async Task<ErrorOr<UserResponse>> CreateAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        var emailExists = await context.Users.AnyAsync(u => u.Email == request.Email, ct);
        if (emailExists)
            return Error.Conflict("User.EmailConflict", $"A user with email '{request.Email}' already exists.");
 
        // IPasswordHasher<User> does not use the User instance for PBKDF2 hashing.
        var passwordHash = passwordHasher.HashPassword(null!, request.Password);
 
        var createResult = User.Create(new UserCreationParams(
            request.NickName,
            request.Email,
            passwordHash,
            Balance: 0,
            Role: Role.User));
 
        if (createResult.IsError)
            return createResult.Errors;
 
        context.Users.Add(createResult.Value);
        await context.SaveChangesAsync(ct);
 
        return ToResponse(createResult.Value);
    }
 
    public async Task<ErrorOr<UserResponse>> UpdateProfileAsync(
        Guid id, UpdateProfileRequest request, CancellationToken ct = default)
    {
        var user = await context.Users.FindAsync([id], ct);
        if (user is null)
            return Error.NotFound("User.NotFound", $"User with id '{id}' was not found.");
 
        var emailTaken = await context.Users
            .AnyAsync(u => u.Email == request.Email && u.Id != id, ct);
 
        if (emailTaken)
            return Error.Conflict("User.EmailConflict", $"Email '{request.Email}' is already taken.");
 
        var updateResult = user.UpdateProfile(request.NickName, request.Email);
        if (updateResult.IsError)
            return updateResult.Errors;
 
        await context.SaveChangesAsync(ct);
        return ToResponse(user);
    }
 
    public async Task<ErrorOr<UserResponse>> ChangeRoleAsync(
        Guid id, ChangeRoleRequest request, CancellationToken ct = default)
    {
        var user = await context.Users.FindAsync([id], ct);
        if (user is null)
            return Error.NotFound("User.NotFound", $"User with id '{id}' was not found.");
 
        if (!Enum.TryParse<Role>(request.Role, ignoreCase: true, out var role))
            return Error.Validation("User.InvalidRole", $"'{request.Role}' is not a valid role. Valid values: {string.Join(", ", Enum.GetNames<Role>())}.");
 
        var changeResult = user.ChangeRole(role);
        if (changeResult.IsError)
            return changeResult.Errors;
 
        await context.SaveChangesAsync(ct);
        return ToResponse(user);
    }
 
    public async Task<ErrorOr<Deleted>> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var user = await context.Users.FindAsync([id], ct);
        if (user is null)
            return Error.NotFound("User.NotFound", $"User with id '{id}' was not found.");
 
        context.Users.Remove(user);
        await context.SaveChangesAsync(ct);
        return Result.Deleted;
    }
 
    private static UserResponse ToResponse(User u) =>
        new(u.Id, u.NickName, u.Email, u.Balance, u.Role.ToString(), u.CreatedAt);
}