namespace CreditCard.IntegrationTests.Repositories;

using CreditCard.Domain.Entities;
using CreditCard.Infrastructure.Persistence;
using CreditCard.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class CreditCardRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly CreditCardDbContext _context;
    private readonly CreditCardRepository _repository;

    public CreditCardRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<CreditCardDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new CreditCardDbContext(options);
        _context.Database.EnsureCreated();
        _repository = new CreditCardRepository(_context);
    }

    #region Pruebas de Integración - AddAsync

    [Fact]
    public async Task AddAsync_ConEntidadValida_DebeGuardarEnBaseDeDatos()
    {
        // Arrange
        var creditCard = CreditCardEntity.Create(
            "4111111111111111",
            "Juan Perez",
            "12/25",
            "123",
            5000m,
            "Visa");

        // Act
        await _repository.AddAsync(creditCard);
        await _context.SaveChangesAsync();

        // Assert
        var savedCard = await _context.CreditCards.FirstOrDefaultAsync(c => c.Id == creditCard.Id);
        savedCard.Should().NotBeNull();
        savedCard!.CardNumber.Should().Be("4111111111111111");
        savedCard.CardHolderName.Should().Be("JUAN PEREZ");
    }

    [Fact]
    public async Task AddAsync_MultiplesTarjetas_DebeGuardarTodas()
    {
        // Arrange
        var card1 = CreditCardEntity.Create("4111111111111111", "Juan Perez", "12/25", "123", 5000m, "Visa");
        var card2 = CreditCardEntity.Create("5500000000000004", "Maria Garcia", "06/26", "456", 8000m, "MasterCard");

        // Act
        await _repository.AddAsync(card1);
        await _repository.AddAsync(card2);
        await _context.SaveChangesAsync();

        // Assert
        var count = await _context.CreditCards.CountAsync();
        count.Should().Be(2);
    }

    #endregion

    #region Pruebas de Integración - GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_ConIdExistente_DebeRetornarEntidad()
    {
        // Arrange
        var creditCard = CreditCardEntity.Create(
            "4111111111111111",
            "Juan Perez",
            "12/25",
            "123",
            5000m,
            "Visa");
        await _context.CreditCards.AddAsync(creditCard);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(creditCard.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(creditCard.Id);
        result.CardNumber.Should().Be("4111111111111111");
    }

    [Fact]
    public async Task GetByIdAsync_ConIdInexistente_DebeRetornarNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Pruebas de Integración - GetAllAsync

    [Fact]
    public async Task GetAllAsync_ConTarjetasExistentes_DebeRetornarTodas()
    {
        // Arrange
        var card1 = CreditCardEntity.Create("4111111111111111", "Juan Perez", "12/25", "123", 5000m, "Visa");
        var card2 = CreditCardEntity.Create("5500000000000004", "Maria Garcia", "06/26", "456", 8000m, "MasterCard");
        await _context.CreditCards.AddRangeAsync(card1, card2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_SinTarjetas_DebeRetornarListaVacia()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_DebeRetornarOrdenadoPorFechaCreacionDescendente()
    {
        // Arrange
        var card1 = CreditCardEntity.Create("4111111111111111", "Juan Perez", "12/25", "123", 5000m, "Visa");
        await Task.Delay(10);
        var card2 = CreditCardEntity.Create("5500000000000004", "Maria Garcia", "06/26", "456", 8000m, "MasterCard");

        await _context.CreditCards.AddAsync(card1);
        await _context.SaveChangesAsync();
        await _context.CreditCards.AddAsync(card2);
        await _context.SaveChangesAsync();

        // Act
        var result = (await _repository.GetAllAsync()).ToList();

        // Assert
        result.First().CardHolderName.Should().Be("MARIA GARCIA"); // Más reciente primero
    }

    #endregion

    #region Pruebas de Integración - GetAllPagedAsync

    [Fact]
    public async Task GetAllPagedAsync_ConTarjetasExistentes_DebeRetornarPaginaCorrecta()
    {
        // Arrange
        var card1 = CreditCardEntity.Create("4111111111111111", "Juan Perez", "12/25", "123", 5000m, "Visa");
        var card2 = CreditCardEntity.Create("5500000000000004", "Maria Garcia", "06/26", "456", 8000m, "MasterCard");
        var card3 = CreditCardEntity.Create("378282246310005", "Carlos Lopez", "03/27", "789", 10000m, "American Express");
        await _context.CreditCards.AddRangeAsync(card1, card2, card3);
        await _context.SaveChangesAsync();

        // Act
        var (items, totalCount) = await _repository.GetAllPagedAsync(1, 2);

        // Assert
        items.Should().HaveCount(2);
        totalCount.Should().Be(3);
    }

    [Fact]
    public async Task GetAllPagedAsync_SegundaPagina_DebeRetornarElementosRestantes()
    {
        // Arrange
        var card1 = CreditCardEntity.Create("4111111111111111", "Juan Perez", "12/25", "123", 5000m, "Visa");
        var card2 = CreditCardEntity.Create("5500000000000004", "Maria Garcia", "06/26", "456", 8000m, "MasterCard");
        var card3 = CreditCardEntity.Create("378282246310005", "Carlos Lopez", "03/27", "789", 10000m, "American Express");
        await _context.CreditCards.AddRangeAsync(card1, card2, card3);
        await _context.SaveChangesAsync();

        // Act
        var (items, totalCount) = await _repository.GetAllPagedAsync(2, 2);

        // Assert
        items.Should().HaveCount(1);
        totalCount.Should().Be(3);
    }

    [Fact]
    public async Task GetAllPagedAsync_SinTarjetas_DebeRetornarVacio()
    {
        // Act
        var (items, totalCount) = await _repository.GetAllPagedAsync(1, 10);

        // Assert
        items.Should().BeEmpty();
        totalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetAllPagedAsync_DebeRetornarOrdenadoPorFechaDescendente()
    {
        // Arrange
        var card1 = CreditCardEntity.Create("4111111111111111", "Juan Perez", "12/25", "123", 5000m, "Visa");
        await _context.CreditCards.AddAsync(card1);
        await _context.SaveChangesAsync();
        
        await Task.Delay(10);
        
        var card2 = CreditCardEntity.Create("5500000000000004", "Maria Garcia", "06/26", "456", 8000m, "MasterCard");
        await _context.CreditCards.AddAsync(card2);
        await _context.SaveChangesAsync();

        // Act
        var (items, _) = await _repository.GetAllPagedAsync(1, 10);
        var itemsList = items.ToList();

        // Assert
        itemsList.First().CardHolderName.Should().Be("MARIA GARCIA"); // Más reciente primero
    }

    #endregion

    #region Pruebas de Integración - GetByCardNumberHashAsync

    [Fact]
    public async Task GetByCardNumberHashAsync_ConHashExistente_DebeRetornarEntidad()
    {
        // Arrange
        var cardNumberHash = "TEST_HASH_4111111111111111";
        var creditCard = CreditCardEntity.Create(
            "4111111111111111",
            "Juan Perez",
            "12/25",
            "123",
            5000m,
            "Visa",
            s => $"TEST_HASH_{s}");
        await _context.CreditCards.AddAsync(creditCard);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByCardNumberHashAsync(cardNumberHash);

        // Assert
        result.Should().NotBeNull();
        result!.CardNumber.Should().Be("4111111111111111");
    }

    [Fact]
    public async Task GetByCardNumberHashAsync_ConHashInexistente_DebeRetornarNull()
    {
        // Act
        var result = await _repository.GetByCardNumberHashAsync("NONEXISTENT_HASH");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Pruebas de Integración - Update

    [Fact]
    public async Task Update_ConCambiosValidos_DebeActualizarEnBaseDeDatos()
    {
        // Arrange
        var creditCard = CreditCardEntity.Create(
            "4111111111111111",
            "Juan Perez",
            "12/25",
            "123",
            5000m,
            "Visa");
        await _context.CreditCards.AddAsync(creditCard);
        await _context.SaveChangesAsync();

        // Act
        creditCard.UpdateCardHolder("Maria Garcia");
        creditCard.UpdateCreditLimit(8000m);
        _repository.Update(creditCard);
        await _context.SaveChangesAsync();

        // Assert
        var updatedCard = await _context.CreditCards.FirstOrDefaultAsync(c => c.Id == creditCard.Id);
        updatedCard!.CardHolderName.Should().Be("MARIA GARCIA");
        updatedCard.CreditLimit.Should().Be(8000m);
    }

    [Fact]
    public async Task Update_ConCargo_DebeReducirCreditoDisponible()
    {
        // Arrange
        var creditCard = CreditCardEntity.Create(
            "4111111111111111",
            "Juan Perez",
            "12/25",
            "123",
            5000m,
            "Visa");
        await _context.CreditCards.AddAsync(creditCard);
        await _context.SaveChangesAsync();

        // Act
        creditCard.MakeCharge(1500m);
        _repository.Update(creditCard);
        await _context.SaveChangesAsync();

        // Assert
        var updatedCard = await _context.CreditCards.FirstOrDefaultAsync(c => c.Id == creditCard.Id);
        updatedCard!.AvailableCredit.Should().Be(3500m);
    }

    [Fact]
    public async Task Update_ConPago_DebeAumentarCreditoDisponible()
    {
        // Arrange
        var creditCard = CreditCardEntity.Create(
            "4111111111111111",
            "Juan Perez",
            "12/25",
            "123",
            5000m,
            "Visa");
        creditCard.MakeCharge(2000m);
        await _context.CreditCards.AddAsync(creditCard);
        await _context.SaveChangesAsync();

        // Act
        creditCard.MakePayment(1000m);
        _repository.Update(creditCard);
        await _context.SaveChangesAsync();

        // Assert
        var updatedCard = await _context.CreditCards.FirstOrDefaultAsync(c => c.Id == creditCard.Id);
        updatedCard!.AvailableCredit.Should().Be(4000m);
    }

    #endregion

    #region Pruebas de Integración - Delete

    [Fact]
    public async Task Delete_ConEntidadExistente_DebeEliminarDeBaseDeDatos()
    {
        // Arrange
        var creditCard = CreditCardEntity.Create(
            "4111111111111111",
            "Juan Perez",
            "12/25",
            "123",
            5000m,
            "Visa");
        await _context.CreditCards.AddAsync(creditCard);
        await _context.SaveChangesAsync();

        // Act
        _repository.Delete(creditCard);
        await _context.SaveChangesAsync();

        // Assert
        var deletedCard = await _context.CreditCards.FirstOrDefaultAsync(c => c.Id == creditCard.Id);
        deletedCard.Should().BeNull();
    }

    #endregion

    #region Pruebas de Integración - Concurrencia

    [Fact]
    public async Task MultipleOperaciones_Concurrentes_DebenMantenerIntegridad()
    {
        // Arrange
        var creditCard = CreditCardEntity.Create(
            "4111111111111111",
            "Juan Perez",
            "12/25",
            "123",
            10000m,
            "Visa");
        await _context.CreditCards.AddAsync(creditCard);
        await _context.SaveChangesAsync();

        // Act - Simular múltiples cargos
        creditCard.MakeCharge(1000m);
        creditCard.MakeCharge(500m);
        creditCard.MakeCharge(1500m);
        _repository.Update(creditCard);
        await _context.SaveChangesAsync();

        // Assert
        var updatedCard = await _context.CreditCards.FirstOrDefaultAsync(c => c.Id == creditCard.Id);
        updatedCard!.AvailableCredit.Should().Be(7000m); // 10000 - 3000
    }

    #endregion

    public void Dispose()
    {
        _context?.Dispose();
        _connection?.Dispose();
        GC.SuppressFinalize(this);
    }
}
