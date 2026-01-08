namespace CreditCard.Infrastructure.Persistence.Configurations;

using CreditCard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CreditCardConfiguration : IEntityTypeConfiguration<CreditCardEntity>
{
    public void Configure(EntityTypeBuilder<CreditCardEntity> builder)
    {
        builder.ToTable("CreditCards");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .ValueGeneratedNever();

        builder.Property(c => c.CardNumber)
            .IsRequired()
            .HasMaxLength(19);

        builder.HasIndex(c => c.CardNumber)
            .IsUnique();

        builder.Property(c => c.CardHolderName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.ExpirationDate)
            .IsRequired()
            .HasMaxLength(7);

        builder.Property(c => c.CVV)
            .IsRequired()
            .HasMaxLength(4);

        builder.Property(c => c.CreditLimit)
            .HasPrecision(18, 2);

        builder.Property(c => c.AvailableCredit)
            .HasPrecision(18, 2);

        builder.Property(c => c.CardType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.IsActive)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.UpdatedAt);
    }
}
