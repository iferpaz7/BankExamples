namespace CreditCard.Application.DTOs;

public record CreateCreditCardDto(
    string CardNumber,
    string CardHolderName,
    string ExpirationDate,
    string CVV,
    decimal CreditLimit,
    string CardType
);

public record UpdateCreditCardDto(
    string CardHolderName,
    decimal CreditLimit
);

public record CreditCardResponseDto(
    Guid Id,
    string CardNumber,
    string CardHolderName,
    string ExpirationDate,
    string CardType,
    decimal CreditLimit,
    decimal AvailableCredit,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record ChargeDto(
    decimal Amount
);

public record PaymentDto(
    decimal Amount
);

public record CreditCardReportDto(
    Guid Id,
    string CardNumber,
    string CardHolderName,
    string CardType,
    decimal CreditLimit,
    decimal AvailableCredit,
    decimal UsedCredit,
    decimal UsagePercentage,
    bool IsActive
);

public record PagedResultDto<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize
)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
