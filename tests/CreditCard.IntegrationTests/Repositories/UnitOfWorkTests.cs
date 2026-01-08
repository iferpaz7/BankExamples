namespace CreditCard.IntegrationTests.Repositories;

using CreditCard.Domain.Entities;
using CreditCard.Infrastructure.Persistence;
using CreditCard.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class UnitOfWorkTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly CreditCardDbContext _context;
    private readonly UnitOfWork _unitOfWork;

    public UnitOfWorkTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<CreditCardDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new CreditCardDbContext(options);
        _context.Database.EnsureCreated();
        _unitOfWork = new UnitOfWork(_context);
    }

    #region Pruebas de Integración - SaveChangesAsync

    [Fact]
    public async Task SaveChangesAsync_ConCambiosPendientes_DebeGuardarEnBaseDeDatos()
    {
        // Arrange
        var creditCard = CreditCardEntity.Create(
            "4111111111111111",
            "Juan Perez",
            "12/25",
            "123",
            5000m,
            "Visa");
        await _unitOfWork.CreditCards.AddAsync(creditCard);

        // Act
        var result = await _unitOfWork.SaveChangesAsync();

        // Assert
        result.Should().Be(1);
        var savedCard = await _context.CreditCards.FirstOrDefaultAsync(c => c.Id == creditCard.Id);
        savedCard.Should().NotBeNull();
    }

    [Fact]
    public async Task SaveChangesAsync_SinCambios_DebeRetornarCero()
    {
        // Act
        var result = await _unitOfWork.SaveChangesAsync();

        // Assert
        result.Should().Be(0);
    }

    #endregion

    #region Pruebas de Integración - CreditCards Repository Access

    [Fact]
    public async Task CreditCards_DebeRetornarRepositorioFuncional()
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
        await _unitOfWork.CreditCards.AddAsync(creditCard);
        await _unitOfWork.SaveChangesAsync();
        var result = await _unitOfWork.CreditCards.GetByIdAsync(creditCard.Id);

        // Assert
        result.Should().NotBeNull();
        result!.CardNumber.Should().Be("4111111111111111");
    }

    [Fact]
    public void CreditCards_MultiplesAccesos_DebeRetornarMismaInstancia()
    {
        // Act
        var repo1 = _unitOfWork.CreditCards;
        var repo2 = _unitOfWork.CreditCards;

        // Assert
        repo1.Should().BeSameAs(repo2);
    }

    #endregion

    #region Pruebas de Integración - Flujo Completo con UnitOfWork

    [Fact]
    public async Task FlujoCompleto_CrearActualizarEliminar_DebeCompletarseExitosamente()
    {
        // 1. Crear
        var creditCard = CreditCardEntity.Create(
            "4111111111111111",
            "Juan Perez",
            "12/25",
            "123",
            5000m,
            "Visa");
        await _unitOfWork.CreditCards.AddAsync(creditCard);
        await _unitOfWork.SaveChangesAsync();

        // 2. Verificar creación
        var created = await _unitOfWork.CreditCards.GetByIdAsync(creditCard.Id);
        created.Should().NotBeNull();

        // 3. Actualizar
        created!.UpdateCardHolder("Maria Garcia");
        created.UpdateCreditLimit(8000m);
        _unitOfWork.CreditCards.Update(created);
        await _unitOfWork.SaveChangesAsync();

        // 4. Verificar actualización
        var updated = await _unitOfWork.CreditCards.GetByIdAsync(creditCard.Id);
        updated!.CardHolderName.Should().Be("MARIA GARCIA");
        updated.CreditLimit.Should().Be(8000m);

        // 5. Eliminar
        _unitOfWork.CreditCards.Delete(updated);
        await _unitOfWork.SaveChangesAsync();

        // 6. Verificar eliminación
        var deleted = await _unitOfWork.CreditCards.GetByIdAsync(creditCard.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task FlujoCompleto_CargosYPagos_DebeActualizarCreditoCorrectamente()
    {
        // Arrange
        var creditCard = CreditCardEntity.Create(
            "4111111111111111",
            "Juan Perez",
            "12/25",
            "123",
            5000m,
            "Visa");
        await _unitOfWork.CreditCards.AddAsync(creditCard);
        await _unitOfWork.SaveChangesAsync();

        // Act - Hacer cargo
        var cardForCharge = await _unitOfWork.CreditCards.GetByIdAsync(creditCard.Id);
        cardForCharge!.MakeCharge(2000m);
        _unitOfWork.CreditCards.Update(cardForCharge);
        await _unitOfWork.SaveChangesAsync();

        // Assert - Verificar cargo
        var afterCharge = await _unitOfWork.CreditCards.GetByIdAsync(creditCard.Id);
        afterCharge!.AvailableCredit.Should().Be(3000m);

        // Act - Hacer pago
        afterCharge.MakePayment(1500m);
        _unitOfWork.CreditCards.Update(afterCharge);
        await _unitOfWork.SaveChangesAsync();

        // Assert - Verificar pago
        var afterPayment = await _unitOfWork.CreditCards.GetByIdAsync(creditCard.Id);
        afterPayment!.AvailableCredit.Should().Be(4500m);
    }

    [Fact]
    public async Task FlujoCompleto_ActivarDesactivar_DebeActualizarEstadoCorrectamente()
    {
        // Arrange
        var creditCard = CreditCardEntity.Create(
            "4111111111111111",
            "Juan Perez",
            "12/25",
            "123",
            5000m,
            "Visa");
        await _unitOfWork.CreditCards.AddAsync(creditCard);
        await _unitOfWork.SaveChangesAsync();

        // Act - Desactivar
        var cardToDeactivate = await _unitOfWork.CreditCards.GetByIdAsync(creditCard.Id);
        cardToDeactivate!.Deactivate();
        _unitOfWork.CreditCards.Update(cardToDeactivate);
        await _unitOfWork.SaveChangesAsync();

        // Assert - Verificar desactivación
        var deactivated = await _unitOfWork.CreditCards.GetByIdAsync(creditCard.Id);
        deactivated!.IsActive.Should().BeFalse();

        // Act - Activar
        deactivated.Activate();
        _unitOfWork.CreditCards.Update(deactivated);
        await _unitOfWork.SaveChangesAsync();

        // Assert - Verificar activación
        var activated = await _unitOfWork.CreditCards.GetByIdAsync(creditCard.Id);
        activated!.IsActive.Should().BeTrue();
    }

    #endregion

    #region Pruebas de Integración - Múltiples Operaciones

    [Fact]
    public async Task MultiplesOperaciones_EnUnaSolaTransaccion_DebeGuardarTodas()
    {
        // Arrange
        var card1 = CreditCardEntity.Create("4111111111111111", "Juan Perez", "12/25", "123", 5000m, "Visa");
        var card2 = CreditCardEntity.Create("5500000000000004", "Maria Garcia", "06/26", "456", 8000m, "MasterCard");
        var card3 = CreditCardEntity.Create("378282246310005", "Pedro Lopez", "03/27", "1234", 10000m, "American Express");

        // Act
        await _unitOfWork.CreditCards.AddAsync(card1);
        await _unitOfWork.CreditCards.AddAsync(card2);
        await _unitOfWork.CreditCards.AddAsync(card3);
        var result = await _unitOfWork.SaveChangesAsync();

        // Assert
        result.Should().Be(3);
        var allCards = await _unitOfWork.CreditCards.GetAllAsync();
        allCards.Should().HaveCount(3);
    }

    #endregion

    public void Dispose()
    {
        _unitOfWork?.Dispose();
        _connection?.Dispose();
        GC.SuppressFinalize(this);
    }
}
