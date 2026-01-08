namespace CreditCard.Application.Services;

using CreditCard.Application.DTOs;
using CreditCard.Application.Interfaces;
using CreditCard.Domain.Entities;
using CreditCard.Domain.Repositories;

public class CreditCardService : ICreditCardService
{
    private readonly IUnitOfWork _unitOfWork;

    public CreditCardService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CreditCardResponseDto> CreateAsync(CreateCreditCardDto dto, CancellationToken cancellationToken = default)
    {
        var existingCard = await _unitOfWork.CreditCards.GetByCardNumberAsync(dto.CardNumber, cancellationToken);
        if (existingCard != null)
            throw new InvalidOperationException("Ya existe una tarjeta con este número");

        var creditCard = CreditCardEntity.Create(
            dto.CardNumber,
            dto.CardHolderName,
            dto.ExpirationDate,
            dto.CVV,
            dto.CreditLimit,
            dto.CardType
        );

        await _unitOfWork.CreditCards.AddAsync(creditCard, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(creditCard);
    }

    public async Task<CreditCardResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var creditCard = await _unitOfWork.CreditCards.GetByIdAsync(id, cancellationToken);
        return creditCard == null ? null : MapToDto(creditCard);
    }

    public async Task<IEnumerable<CreditCardResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var creditCards = await _unitOfWork.CreditCards.GetAllAsync(cancellationToken);
        return creditCards.Select(MapToDto);
    }

    public async Task<PagedResultDto<CreditCardResponseDto>> GetAllPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.CreditCards.GetAllPagedAsync(pageNumber, pageSize, cancellationToken);
        return new PagedResultDto<CreditCardResponseDto>(
            items.Select(MapToDto),
            totalCount,
            pageNumber,
            pageSize
        );
    }

    public async Task<CreditCardResponseDto> UpdateAsync(Guid id, UpdateCreditCardDto dto, CancellationToken cancellationToken = default)
    {
        var creditCard = await _unitOfWork.CreditCards.GetByIdAsync(id, cancellationToken);
        if (creditCard == null)
            throw new InvalidOperationException("Tarjeta no encontrada");

        creditCard.UpdateCardHolder(dto.CardHolderName);
        creditCard.UpdateCreditLimit(dto.CreditLimit);

        _unitOfWork.CreditCards.Update(creditCard);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(creditCard);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var creditCard = await _unitOfWork.CreditCards.GetByIdAsync(id, cancellationToken);
        if (creditCard == null)
            throw new InvalidOperationException("Tarjeta no encontrada");

        _unitOfWork.CreditCards.Delete(creditCard);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<CreditCardResponseDto> MakeChargeAsync(Guid id, ChargeDto dto, CancellationToken cancellationToken = default)
    {
        var creditCard = await _unitOfWork.CreditCards.GetByIdAsync(id, cancellationToken);
        if (creditCard == null)
            throw new InvalidOperationException("Tarjeta no encontrada");

        creditCard.MakeCharge(dto.Amount);
        _unitOfWork.CreditCards.Update(creditCard);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(creditCard);
    }

    public async Task<CreditCardResponseDto> MakePaymentAsync(Guid id, PaymentDto dto, CancellationToken cancellationToken = default)
    {
        var creditCard = await _unitOfWork.CreditCards.GetByIdAsync(id, cancellationToken);
        if (creditCard == null)
            throw new InvalidOperationException("Tarjeta no encontrada");

        creditCard.MakePayment(dto.Amount);
        _unitOfWork.CreditCards.Update(creditCard);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(creditCard);
    }

    public async Task<CreditCardResponseDto> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var creditCard = await _unitOfWork.CreditCards.GetByIdAsync(id, cancellationToken);
        if (creditCard == null)
            throw new InvalidOperationException("Tarjeta no encontrada");

        creditCard.Deactivate();
        _unitOfWork.CreditCards.Update(creditCard);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(creditCard);
    }

    public async Task<CreditCardResponseDto> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var creditCard = await _unitOfWork.CreditCards.GetByIdAsync(id, cancellationToken);
        if (creditCard == null)
            throw new InvalidOperationException("Tarjeta no encontrada");

        creditCard.Activate();
        _unitOfWork.CreditCards.Update(creditCard);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(creditCard);
    }

    private static CreditCardResponseDto MapToDto(CreditCardEntity entity)
    {
        return new CreditCardResponseDto(
            entity.Id,
            entity.CardNumber,
            entity.CardHolderName,
            entity.ExpirationDate,
            entity.CardType,
            entity.CreditLimit,
            entity.AvailableCredit,
            entity.IsActive,
            entity.CreatedAt,
            entity.UpdatedAt
        );
    }
}
