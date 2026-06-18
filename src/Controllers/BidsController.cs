using BidPulse.Contracts.Bids;
using BidPulse.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BidPulse.Controllers;

public sealed class BidsController(IBidService bidService) : ApiController
{
    /// <summary>Returns all bids for a given auction lot, sorted by amount descending.</summary>
    [HttpGet("lot/{lotId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<BidResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByLot(Guid lotId, CancellationToken ct)
    {
        var result = await bidService.GetByLotIdAsync(lotId, ct);
        return result.Match(Ok, Problem);
    }
 
    /// <summary>Returns all bids placed by a specific user.</summary>
    [HttpGet("user/{userId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(List<BidResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByUser(Guid userId, CancellationToken ct)
    {
        var result = await bidService.GetByUserIdAsync(userId, ct);
        return result.Match(Ok, Problem);
    }
 
    /// <summary>
    /// Places a new bid on an active auction lot.
    /// Freezes the bidder's funds and refunds the previous top bidder automatically.
    /// The bidder identity is resolved from the JWT token.
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(BidResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PlaceBid([FromBody] PlaceBidRequest request, CancellationToken ct)
    {
        var result = await bidService.PlaceBidAsync(request, ct);
        return result.Match(
            bid => CreatedAtAction(nameof(GetByLot), new { lotId = bid.LotId }, bid),
            Problem);
    }
}