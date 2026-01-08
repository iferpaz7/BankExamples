namespace CreditCard.Application.Interfaces;

using CreditCard.Application.DTOs;

public interface ICreditCardReportService
{
    Task<IEnumerable<CreditCardReportDto>> GetCreditCardsReportAsync(CancellationToken cancellationToken = default);
    Task<CreditCardReportDto?> GetCreditCardReportByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<CreditCardReportDto>> GetActiveCreditCardsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<CreditCardReportDto>> GetCreditCardsWithHighUsageAsync(decimal minPercentage, CancellationToken cancellationToken = default);
}
