using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BidPulse.Contracts.Auth;
using BidPulse.Contracts.Users;
using BidPulse.Database;
using BidPulse.Database.Entities;
using BidPulse.Services.Abstractions;
using BidPulse.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ErrorOr;
using Microsoft.IdentityModel.Tokens;

namespace BidPulse.Services.Implementations;

public sealed class AuthService(
    AppDbContext context,
    IUserService userService,
    IPasswordHasher<User> passwordHasher,
    IOptions<JwtSettings> jwtOptions) : IAuthService
{
    private readonly JwtSettings _jwt = jwtOptions.Value;
 
    public async Task<ErrorOr<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var createResult = await userService.CreateAsync(
            new CreateUserRequest(request.NickName, request.Email, request.Password), ct);

        if (createResult.IsError)
            return createResult.Errors;

        var user = createResult.Value;
        var token = GenerateJwtToken(user.Id, user.Email, user.NickName, user.Role);

        return new AuthResponse(token, user.Id, user.NickName, user.Role);
    }
    
    public async Task<ErrorOr<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == request.Email, ct);
 
        if (user is null)
            return Error.Unauthorized("Auth.InvalidCredentials", "Invalid email or password.");
 
        // IPasswordHasher<User> does not use the User instance when verifying PBKDF2 hashes.
        var verifyResult = passwordHasher.VerifyHashedPassword(null!, user.PasswordHash, request.Password);
        if (verifyResult == PasswordVerificationResult.Failed)
            return Error.Unauthorized("Auth.InvalidCredentials", "Invalid email or password.");
 
        var token = GenerateJwtToken(user.Id, user.Email, user.NickName, user.Role.ToString());
        return new AuthResponse(token, user.Id, user.NickName, user.Role.ToString());
    }
 
    private string GenerateJwtToken(Guid userId, string email, string nickName, string role)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
 
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Name, nickName),
            new(ClaimTypes.Role, role)
        };
 
        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(_jwt.ExpirationDays),
            signingCredentials: credentials);
 
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}