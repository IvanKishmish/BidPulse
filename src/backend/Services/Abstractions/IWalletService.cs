using BidPulse.Contracts.Wallet;
using ErrorOr;

namespace BidPulse.Services.Abstractions;

public interface IWalletService
{
    Task<ErrorOr<List<WalletTransactionResponse>>> GetTransactionHistoryAsync(CancellationToken ct = default);
    Task<ErrorOr<WalletTransactionResponse>> DepositAsync(DepositRequest request, CancellationToken ct = default);
    Task<ErrorOr<WalletTransactionResponse>> WithdrawAsync(WithdrawRequest request, CancellationToken ct = default);
}