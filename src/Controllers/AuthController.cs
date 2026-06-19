using BidPulse.Contracts.Auth;
using BidPulse.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace BidPulse.Controllers;

public sealed class AuthController(IAuthService authService) : ApiController
{
    /// <summary>
    /// Registers a new user account and returns a JWT bearer token.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken ct)
    {
        var result = await authService.RegisterAsync(request, ct);
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Authenticates a user and returns a JWT bearer token.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct)
    {
        var result = await authService.LoginAsync(request, ct);
        return result.Match(Ok, Problem);
    }

}