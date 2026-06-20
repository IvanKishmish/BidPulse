namespace BidPulse.Database.Entities.Common;

public interface IAuditable
{
    DateTimeOffset CreatedAt { get;} 
    Guid CreatedBy { get;}
    
    DateTimeOffset? UpdatedAt { get;}
    Guid? UpdatedBy { get;}
}