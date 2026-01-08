namespace CreditCard.Application.Services;

using CreditCard.Application.DTOs;
using CreditCard.Application.Interfaces;
using CreditCard.Domain.Repositories;

public class CreditCardReportService : ICreditCardReportService
{
    private readonly ICreditCardReadRepository _readRepository;

    public CreditCardReportService(ICreditCardReadRepository readRepository)
    {
        _readRepository = readRepository;
    }

    public async Task<IEnumerable<CreditCardReportDto>> GetCreditCardsReportAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                Id,
                CardNumber,
                CardHolderName,
                CardType,
                CreditLimit,
                AvailableCredit,
                (CreditLimit - AvailableCredit) as UsedCredit,
                CAST(((CreditLimit - AvailableCredit) * 100.0 / CreditLimit) AS REAL) as UsagePercentage,
                IsActive
            FROM CreditCards
            ORDER BY CreatedAt DESC";

        var results = await _readRepository.QueryAsync<CreditCardReportDto>(sql, null, cancellationToken);
        return results;
    }

    public async Task<CreditCardReportDto?> GetCreditCardReportByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                Id,
                CardNumber,
                CardHolderName,
                CardType,
                CreditLimit,
                AvailableCredit,
                (CreditLimit - AvailableCredit) as UsedCredit,
                CAST(((CreditLimit - AvailableCredit) * 100.0 / CreditLimit) AS REAL) as UsagePercentage,
                IsActive
            FROM CreditCards
            WHERE Id = @Id";

        return await _readRepository.QueryFirstOrDefaultAsync<CreditCardReportDto>(sql, new { Id = id.ToString() }, cancellationToken);
    }

    public async Task<IEnumerable<CreditCardReportDto>> GetActiveCreditCardsAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                Id,
                CardNumber,
                CardHolderName,
                CardType,
                CreditLimit,
                AvailableCredit,
                (CreditLimit - AvailableCredit) as UsedCredit,
                CAST(((CreditLimit - AvailableCredit) * 100.0 / CreditLimit) AS REAL) as UsagePercentage,
                IsActive
            FROM CreditCards
            WHERE IsActive = 1
            ORDER BY CreatedAt DESC";

        var results = await _readRepository.QueryAsync<CreditCardReportDto>(sql, null, cancellationToken);
        return results;
    }

    public async Task<IEnumerable<CreditCardReportDto>> GetCreditCardsWithHighUsageAsync(decimal minPercentage, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                Id,
                CardNumber,
                CardHolderName,
                CardType,
                CreditLimit,
                AvailableCredit,
                (CreditLimit - AvailableCredit) as UsedCredit,
                CAST(((CreditLimit - AvailableCredit) * 100.0 / CreditLimit) AS REAL) as UsagePercentage,
                IsActive
            FROM CreditCards
            WHERE ((CreditLimit - AvailableCredit) * 100.0 / CreditLimit) >= @MinPercentage
            ORDER BY UsagePercentage DESC";

        var results = await _readRepository.QueryAsync<CreditCardReportDto>(sql, new { MinPercentage = minPercentage }, cancellationToken);
        return results;
    }
}
