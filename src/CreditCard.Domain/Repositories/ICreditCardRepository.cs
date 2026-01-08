namespace CreditCard.Domain.Repositories;

using CreditCard.Domain.Entities;

public interface ICreditCardRepository
{
    Task<CreditCardEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<CreditCardEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<(IEnumerable<CreditCardEntity> Items, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<CreditCardEntity?> GetByCardNumberAsync(string cardNumber, CancellationToken cancellationToken = default);
    Task AddAsync(CreditCardEntity creditCard, CancellationToken cancellationToken = default);
    void Update(CreditCardEntity creditCard);
    void Delete(CreditCardEntity creditCard);
}
