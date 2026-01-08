namespace CreditCard.Infrastructure.Persistence;

using CreditCard.Domain.Entities;
using CreditCard.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

public class CreditCardDbContext : DbContext
{
    private readonly IEncryptionService? _encryptionService;

    public CreditCardDbContext(DbContextOptions<CreditCardDbContext> options) : base(options)
    {
    }

    public CreditCardDbContext(DbContextOptions<CreditCardDbContext> options, IEncryptionService encryptionService) 
        : base(options)
    {
        _encryptionService = encryptionService;
    }

    public DbSet<CreditCardEntity> CreditCards => Set<CreditCardEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CreditCardDbContext).Assembly);

        // Apply encryption to sensitive fields if encryption service is available
        if (_encryptionService != null)
        {
            var encryptedConverter = new EncryptedStringConverter(_encryptionService);

            modelBuilder.Entity<CreditCardEntity>(entity =>
            {
                // Encrypt sensitive credit card fields
                entity.Property(e => e.CardNumber).HasConversion(encryptedConverter);
                entity.Property(e => e.CVV).HasConversion(encryptedConverter);
                entity.Property(e => e.ExpirationDate).HasConversion(encryptedConverter);
            });
        }
    }
}
