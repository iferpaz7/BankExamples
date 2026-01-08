namespace CreditCard.IntegrationTests.Fixtures;

using CreditCard.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

/// <summary>
/// Fixture base para pruebas de integración de repositorios
/// </summary>
public class DatabaseFixture : IDisposable
{
    private readonly SqliteConnection _connection;
    public CreditCardDbContext Context { get; private set; }
    public IServiceProvider ServiceProvider { get; private set; }

    public DatabaseFixture()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var services = new ServiceCollection();

        services.AddDbContext<CreditCardDbContext>(options =>
            options.UseSqlite(_connection));

        ServiceProvider = services.BuildServiceProvider();
        Context = ServiceProvider.GetRequiredService<CreditCardDbContext>();
        Context.Database.EnsureCreated();
    }

    public CreditCardDbContext CreateNewContext()
    {
        var options = new DbContextOptionsBuilder<CreditCardDbContext>()
            .UseSqlite(_connection)
            .Options;

        return new CreditCardDbContext(options);
    }

    public void Dispose()
    {
        Context?.Dispose();
        _connection?.Dispose();
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Collection fixture para compartir la base de datos entre pruebas
/// </summary>
[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
}
