namespace CreditCard.Infrastructure.Persistence.Repositories;

using CreditCard.Domain.Entities;
using CreditCard.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

public class CreditCardRepository : ICreditCardRepository
{
    private readonly CreditCardDbContext _context;

    public CreditCardRepository(CreditCardDbContext context)
    {
        _context = context;
    }

    public async Task<CreditCardEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.CreditCards
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<CreditCardEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.CreditCards
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<CreditCardEntity> Items, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var totalCount = await _context.CreditCards.CountAsync(cancellationToken);
        
        var items = await _context.CreditCards
            .OrderByDescending(c => c.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<CreditCardEntity?> GetByCardNumberAsync(string cardNumber, CancellationToken cancellationToken = default)
    {
        return await _context.CreditCards
            .FirstOrDefaultAsync(c => c.CardNumber == cardNumber, cancellationToken);
    }

    public async Task AddAsync(CreditCardEntity creditCard, CancellationToken cancellationToken = default)
    {
        await _context.CreditCards.AddAsync(creditCard, cancellationToken);
    }

    public void Update(CreditCardEntity creditCard)
    {
        _context.CreditCards.Update(creditCard);
    }

    public void Delete(CreditCardEntity creditCard)
    {
        _context.CreditCards.Remove(creditCard);
    }
}
