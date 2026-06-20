using BidPulse.Contracts.AuctionLots;
using ErrorOr;

namespace BidPulse.Services.Abstractions;

public interface IAuctionLotService
{
    Task<ErrorOr<List<AuctionLotResponse>>> GetAllAsync(CancellationToken ct = default);
    Task<ErrorOr<AuctionLotResponse>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ErrorOr<AuctionLotResponse>> CreateAsync(CreateAuctionLotRequest request, CancellationToken ct = default);
    Task<ErrorOr<AuctionLotResponse>> UpdateAsync(Guid id, UpdateAuctionLotRequest request, CancellationToken ct = default);
    Task<ErrorOr<Deleted>> CancelAsync(Guid id, CancellationToken ct = default);
}