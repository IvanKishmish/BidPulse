using BidPulse.Contracts.AuctionLots;
using BidPulse.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BidPulse.Controllers;

public sealed class AuctionLotsController(IAuctionLotService lotService) : ApiController
{
    /// <summary>Returns all auction lots.</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<AuctionLotResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await lotService.GetAllAsync(ct);
        return result.Match(Ok, Problem);
    }
 
    /// <summary>Returns a single auction lot by id.</summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuctionLotResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await lotService.GetByIdAsync(id, ct);
        return result.Match(Ok, Problem);
    }
 
    /// <summary>Creates a new auction lot. The creator is taken from the JWT token.</summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(AuctionLotResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateAuctionLotRequest request, CancellationToken ct)
    {
        var result = await lotService.CreateAsync(request, ct);
        return result.Match(
            lot => CreatedAtAction(nameof(GetById), new { id = lot.Id }, lot),
            Problem);
    }
 
    /// <summary>Updates an existing auction lot. Only the creator can update it.</summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(AuctionLotResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateAuctionLotRequest request,
        CancellationToken ct)
    {
        var result = await lotService.UpdateAsync(id, request, ct);
        return result.Match(Ok, Problem);
    }
 
    /// <summary>Cancels an auction lot and refunds the current top bidder. Only the creator can cancel.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        var result = await lotService.CancelAsync(id, ct);
        return result.Match(_ => NoContent(), Problem);
    }
}