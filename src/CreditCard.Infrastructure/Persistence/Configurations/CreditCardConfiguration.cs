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

        // CardNumber is encrypted, so we need more space for the encrypted value
        builder.Property(c => c.CardNumber)
            .IsRequired()
            .HasMaxLength(256);

        // CardNumberHash is used for unique constraint instead of CardNumber
        // since the encrypted CardNumber uses random IV and won't be searchable
        builder.Property(c => c.CardNumberHash)
            .IsRequired()
            .HasMaxLength(64);

        builder.HasIndex(c => c.CardNumberHash)
            .IsUnique();

        builder.Property(c => c.CardHolderName)
            .IsRequired()
            .HasMaxLength(100);

        // ExpirationDate is encrypted, needs more space
        builder.Property(c => c.ExpirationDate)
            .IsRequired()
            .HasMaxLength(128);

        // CVV is encrypted, needs more space
        builder.Property(c => c.CVV)
            .IsRequired()
            .HasMaxLength(128);

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
