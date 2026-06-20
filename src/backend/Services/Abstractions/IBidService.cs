using BidPulse.Contracts.Bids;
using ErrorOr;

namespace BidPulse.Services.Abstractions;

public interface IBidService
{
    Task<ErrorOr<List<BidResponse>>> GetByLotIdAsync(Guid lotId, CancellationToken ct = default);
    Task<ErrorOr<List<BidResponse>>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<ErrorOr<BidResponse>> PlaceBidAsync(PlaceBidRequest request, CancellationToken ct = default);
}