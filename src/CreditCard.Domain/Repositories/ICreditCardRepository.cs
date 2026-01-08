namespace CreditCard.Domain.Repositories;

using CreditCard.Domain.Entities;

public interface ICreditCardRepository
{
    Task<CreditCardEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<CreditCardEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<(IEnumerable<CreditCardEntity> Items, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    /// <summary>
    /// Gets a credit card by its card number hash.
    /// </summary>
    /// <param name="cardNumberHash">The HMAC hash of the card number</param>
    Task<CreditCardEntity?> GetByCardNumberHashAsync(string cardNumberHash, CancellationToken cancellationToken = default);
    Task AddAsync(CreditCardEntity creditCard, CancellationToken cancellationToken = default);
    void Update(CreditCardEntity creditCard);
    void Delete(CreditCardEntity creditCard);
}
