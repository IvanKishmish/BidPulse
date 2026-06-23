using BidPulse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BidPulse.Persistence.EntityTypeConfigurations;

public sealed class AuctionLotConfiguration : IEntityTypeConfiguration<AuctionLot>
{
    public void Configure(EntityTypeBuilder<AuctionLot> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(x => x.StartPrice).HasPrecision(18, 2);
        builder.Property(x => x.CurrentPrice).HasPrecision(18, 2);
        builder.Property(x => x.MinBidStep).HasPrecision(18, 2);

        builder.HasOne(x => x.Category)
            .WithMany() 
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.User)
            .WithMany() 
            .HasForeignKey(x => x.CreatorId)
            .OnDelete(DeleteBehavior.Restrict); 
    }
}