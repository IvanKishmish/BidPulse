namespace BidPulse.Services.Abstractions;

public interface ICurrentUserProvider
{
    Guid?  UserId { get;}
}