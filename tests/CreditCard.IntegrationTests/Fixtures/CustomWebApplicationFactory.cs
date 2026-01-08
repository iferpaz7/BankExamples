namespace CreditCard.IntegrationTests.Fixtures;

using CreditCard.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remover todos los DbContextOptions registrados
            services.RemoveAll(typeof(DbContextOptions<CreditCardDbContext>));
            services.RemoveAll(typeof(DbContextOptions));

            // Remover el CreditCardReadRepository existente
            var readRepoDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(Domain.Repositories.ICreditCardReadRepository));

            if (readRepoDescriptor != null)
            {
                services.Remove(readRepoDescriptor);
            }

            // Crear conexión SQLite en memoria
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            // Agregar DbContext con SQLite InMemory
            services.AddDbContext<CreditCardDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });

            // Agregar ReadRepository para pruebas usando SQLite
            services.AddScoped<Domain.Repositories.ICreditCardReadRepository>(sp =>
                new TestCreditCardReadRepository(_connection.ConnectionString));

            // Crear la base de datos
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CreditCardDbContext>();
            db.Database.EnsureCreated();
        });

        builder.UseEnvironment("Testing");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection?.Dispose();
        }
    }
}

/// <summary>
/// Implementación del ReadRepository para pruebas con SQLite
/// </summary>
public class TestCreditCardReadRepository : Domain.Repositories.ICreditCardReadRepository
{
    private readonly string _connectionString;

    public TestCreditCardReadRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? param = null, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        return default;
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        return Enumerable.Empty<T>();
    }
}
