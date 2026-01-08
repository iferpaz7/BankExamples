namespace CreditCard.Infrastructure.Persistence;

using CreditCard.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public class CreditCardDbContext : DbContext
{
    public CreditCardDbContext(DbContextOptions<CreditCardDbContext> options) : base(options)
    {
    }

    public DbSet<CreditCardEntity> CreditCards => Set<CreditCardEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CreditCardDbContext).Assembly);
    }
}
