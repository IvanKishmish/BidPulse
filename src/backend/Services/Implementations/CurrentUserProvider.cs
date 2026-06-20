using System.Security.Claims;
using BidPulse.Services.Abstractions;

namespace BidPulse.Services.Implementations;

public sealed class CurrentUserProvider(IHttpContextAccessor httpContextAccessor) : ICurrentUserProvider
{
    // public Guid? UserId
    // {
    //     get
    //     {
    //         var claim = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
    //         return claim is not null && Guid.TryParse(claim.Value, out var id) ? id : null;
    //     }
    // }
    
    
    /// <summary>
    /// Gets the unique identifier (GUID) of the currently authenticated user.
    /// </summary>
    /// <value>
    /// A <see cref="Guid"/> representing the user's ID if authenticated and the claim is valid; 
    /// otherwise, <see langword="null"/> if the user is anonymous, not authenticated, or the claim is missing/malformed.
    /// </value>
    /// <remarks>
    /// <b>How it works:</b>
    /// <list type="number">
    /// <item>
    /// <description>
    /// It uses safe navigation (<c>?.</c>) to access the <see cref="HttpContext.User"/> object, ensuring no <see cref="NullReferenceException"/> occurs if the HTTP context is missing.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// The <c>FindFirstValue</c> extension method searches for the first claim matching <see cref="ClaimTypes.NameIdentifier"/> (commonly maps to the <c>sub</c> claim in JWTs) and directly extracts its string value.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <see cref="Guid.TryParse(string?, out Guid)"/> safely evaluates whether the retrieved string is a valid GUID representation.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// A ternary operator returns the parsed <c>userId</c> upon success, or <see langword="null"/> if any step fails.
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
    public Guid? UserId =>
        Guid.TryParse(httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)
            ? userId
            : null;
}