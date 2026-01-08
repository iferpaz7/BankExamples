namespace CreditCard.Infrastructure.Persistence.Repositories;

using CreditCard.Domain.Repositories;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

public class CreditCardReadRepository : ICreditCardReadRepository
{
    private readonly string _connectionString;

    public CreditCardReadRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? param = null, CancellationToken cancellationToken = default)
    {
        using var connection = new SqliteConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<T>(sql, param);
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null, CancellationToken cancellationToken = default)
    {
        using var connection = new SqliteConnection(_connectionString);
        return await connection.QueryAsync<T>(sql, param);
    }
}
