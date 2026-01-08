namespace CreditCard.Application.Interfaces;

using CreditCard.Application.DTOs;

public interface ICreditCardService
{
    Task<CreditCardResponseDto> CreateAsync(CreateCreditCardDto dto, CancellationToken cancellationToken = default);
    Task<CreditCardResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<CreditCardResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PagedResultDto<CreditCardResponseDto>> GetAllPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<CreditCardResponseDto> UpdateAsync(Guid id, UpdateCreditCardDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CreditCardResponseDto> MakeChargeAsync(Guid id, ChargeDto dto, CancellationToken cancellationToken = default);
    Task<CreditCardResponseDto> MakePaymentAsync(Guid id, PaymentDto dto, CancellationToken cancellationToken = default);
    Task<CreditCardResponseDto> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CreditCardResponseDto> ActivateAsync(Guid id, CancellationToken cancellationToken = default);
}
