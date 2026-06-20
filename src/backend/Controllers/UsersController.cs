using BidPulse.Contracts.Users;
using BidPulse.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BidPulse.Controllers;

public sealed class UsersController(IUserService userService) : ApiController
{
    /// <summary>Returns all registered users. Admin only.</summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(List<UserResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await userService.GetAllAsync(ct);
        return result.Match(Ok, Problem);
    }
 
    /// <summary>Returns a single user by their id.</summary>
    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await userService.GetByIdAsync(id, ct);
        return result.Match(Ok, Problem);
    }
 
    /// <summary>Creates a user account directly. Admin only — for self-registration use POST /api/auth/register.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken ct)
    {
        var result = await userService.CreateAsync(request, ct);
        return result.Match(
            user => CreatedAtAction(nameof(GetById), new { id = user.Id }, user),
            Problem);
    }
 
    /// <summary>Updates nickname and email of an existing user.</summary>
    [HttpPut("{id:guid}/profile")]
    [Authorize]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProfile(
        Guid id,
        [FromBody] UpdateProfileRequest request,
        CancellationToken ct)
    {
        var result = await userService.UpdateProfileAsync(id, request, ct);
        return result.Match(Ok, Problem);
    }
 
    /// <summary>Changes the role of a user. Admin only.</summary>
    [HttpPut("{id:guid}/role")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeRole(
        Guid id,
        [FromBody] ChangeRoleRequest request,
        CancellationToken ct)
    {
        var result = await userService.ChangeRoleAsync(id, request, ct);
        return result.Match(Ok, Problem);
    }
 
    /// <summary>Permanently deletes a user account. Admin only.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await userService.DeleteAsync(id, ct);
        return result.Match(_ => NoContent(), Problem);
    }

}