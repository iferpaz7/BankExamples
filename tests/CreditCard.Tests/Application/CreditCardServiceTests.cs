namespace CreditCard.Tests.Application;

using CreditCard.Application.DTOs;
using CreditCard.Application.Interfaces;
using CreditCard.Application.Services;
using CreditCard.Domain.Entities;
using CreditCard.Domain.Repositories;
using FluentAssertions;
using Moq;
using Xunit;

public class CreditCardServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICreditCardRepository> _repositoryMock;
    private readonly Mock<IHashService> _hashServiceMock;
    private readonly CreditCardService _service;

    public CreditCardServiceTests()
    {
        _repositoryMock = new Mock<ICreditCardRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _hashServiceMock = new Mock<IHashService>();
        _unitOfWorkMock.Setup(x => x.CreditCards).Returns(_repositoryMock.Object);
        
        // Setup hash service to return predictable hashes
        _hashServiceMock.Setup(x => x.ComputeHash(It.IsAny<string>()))
            .Returns<string>(s => $"HASH_{s}");
        
        _service = new CreditCardService(_unitOfWorkMock.Object, _hashServiceMock.Object);
    }

    #region Pruebas Positivas - CreateAsync

    [Fact]
    public async Task CreateAsync_ConDatosValidos_DebeCrearTarjetaYRetornarDto()
    {
        // Arrange
        var dto = new CreateCreditCardDto(
            "4111111111111111",
            "Juan Perez",
            "12/25",
            "123",
            5000m,
            "Visa");

        _repositoryMock.Setup(x => x.GetByCardNumberHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CreditCardEntity?)null);

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.CardNumber.Should().Be(dto.CardNumber);
        result.CardHolderName.Should().Be("JUAN PEREZ");
        result.CreditLimit.Should().Be(dto.CreditLimit);
        result.AvailableCredit.Should().Be(dto.CreditLimit);
        result.IsActive.Should().BeTrue();

        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<CreditCardEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _hashServiceMock.Verify(x => x.ComputeHash(dto.CardNumber), Times.AtLeastOnce);
    }

    #endregion

    #region Pruebas Negativas - CreateAsync

    [Fact]
    public async Task CreateAsync_ConNumeroTarjetaExistente_DebeLanzarExcepcion()
    {
        // Arrange
        var dto = new CreateCreditCardDto(
            "4111111111111111",
            "Juan Perez",
            "12/25",
            "123",
            5000m,
            "Visa");

        var existingCard = CreditCardEntity.Create(
            dto.CardNumber, "Otro Titular", "01/26", "456", 3000m, "Visa");

        _repositoryMock.Setup(x => x.GetByCardNumberHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCard);

        // Act
        var act = async () => await _service.CreateAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Ya existe una tarjeta con este número");

        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<CreditCardEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Pruebas Positivas - GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_ConIdExistente_DebeRetornarDto()
    {
        // Arrange
        var creditCard = CreditCardEntity.Create(
            "4111111111111111", "Juan Perez", "12/25", "123", 5000m, "Visa");

        _repositoryMock.Setup(x => x.GetByIdAsync(creditCard.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(creditCard);

        // Act
        var result = await _service.GetByIdAsync(creditCard.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(creditCard.Id);
        result.CardNumber.Should().Be(creditCard.CardNumber);
    }

    #endregion

    #region Pruebas Negativas - GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_ConIdInexistente_DebeRetornarNull()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repositoryMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CreditCardEntity?)null);

        // Act
        var result = await _service.GetByIdAsync(id);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Pruebas Positivas - GetAllAsync

    [Fact]
    public async Task GetAllAsync_ConTarjetasExistentes_DebeRetornarListaDeDtos()
    {
        // Arrange
        var cards = new List<CreditCardEntity>
        {
            CreditCardEntity.Create("4111111111111111", "Juan Perez", "12/25", "123", 5000m, "Visa"),
            CreditCardEntity.Create("5500000000000004", "Maria Garcia", "06/26", "456", 8000m, "MasterCard")
        };

        _repositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(cards);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.First().CardHolderName.Should().Be("JUAN PEREZ");
        result.Last().CardHolderName.Should().Be("MARIA GARCIA");
    }

    [Fact]
    public async Task GetAllAsync_SinTarjetas_DebeRetornarListaVacia()
    {
        // Arrange
        _repositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CreditCardEntity>());

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region Pruebas Positivas - GetAllPagedAsync

    [Fact]
    public async Task GetAllPagedAsync_ConTarjetasExistentes_DebeRetornarResultadoPaginado()
    {
        // Arrange
        var cards = new List<CreditCardEntity>
        {
            CreditCardEntity.Create("4111111111111111", "Juan Perez", "12/25", "123", 5000m, "Visa"),
            CreditCardEntity.Create("5500000000000004", "Maria Garcia", "06/26", "456", 8000m, "MasterCard"),
            CreditCardEntity.Create("378282246310005", "Carlos Lopez", "03/27", "789", 10000m, "American Express")
        };

        _repositoryMock.Setup(x => x.GetAllPagedAsync(1, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync((cards.Take(2), 3));

        // Act
        var result = await _service.GetAllPagedAsync(1, 2);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(3);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(2);
        result.TotalPages.Should().Be(2);
        result.HasNextPage.Should().BeTrue();
        result.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllPagedAsync_SegundaPagina_DebeRetornarElementosCorrectos()
    {
        // Arrange
        var card3 = CreditCardEntity.Create("378282246310005", "Carlos Lopez", "03/27", "789", 10000m, "American Express");

        _repositoryMock.Setup(x => x.GetAllPagedAsync(2, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<CreditCardEntity> { card3 }, 3));

        // Act
        var result = await _service.GetAllPagedAsync(2, 2);

        // Assert
        result.Items.Should().HaveCount(1);
        result.PageNumber.Should().Be(2);
        result.HasPreviousPage.Should().BeTrue();
        result.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllPagedAsync_SinTarjetas_DebeRetornarResultadoVacio()
    {
        // Arrange
        _repositoryMock.Setup(x => x.GetAllPagedAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<CreditCardEntity>(), 0));

        // Act
        var result = await _service.GetAllPagedAsync(1, 10);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    #endregion

    #region Pruebas Positivas - UpdateAsync

    [Fact]
    public async Task UpdateAsync_ConDatosValidos_DebeActualizarYRetornarDto()
    {
        // Arrange
        var creditCard = CreditCardEntity.Create(
            "4111111111111111", "Juan Perez", "12/25", "123", 5000m, "Visa");

        var updateDto = new UpdateCreditCardDto("Maria Garcia", 8000m);

        _repositoryMock.Setup(x => x.GetByIdAsync(creditCard.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(creditCard);

        // Act
        var result = await _service.UpdateAsync(creditCard.Id, updateDto);

        // Assert
        result.CardHolderName.Should().Be("MARIA GARCIA");
        result.CreditLimit.Should().Be(8000m);

        _repositoryMock.Verify(x => x.Update(creditCard), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Pruebas Negativas - UpdateAsync

    [Fact]
    public async Task UpdateAsync_ConIdInexistente_DebeLanzarExcepcion()
    {
        // Arrange
        var id = Guid.NewGuid();
        var updateDto = new UpdateCreditCardDto("Maria Garcia", 8000m);

        _repositoryMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CreditCardEntity?)null);

        // Act
        var act = async () => await _service.UpdateAsync(id, updateDto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Tarjeta no encontrada");
    }

    #endregion

    #region Pruebas Positivas - DeleteAsync

    [Fact]
    public async Task DeleteAsync_ConIdExistente_DebeEliminarTarjeta()
    {
        // Arrange
        var creditCard = CreditCardEntity.Create(
            "4111111111111111", "Juan Perez", "12/25", "123", 5000m, "Visa");

        _repositoryMock.Setup(x => x.GetByIdAsync(creditCard.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(creditCard);

        // Act
        await _service.DeleteAsync(creditCard.Id);

        // Assert
        _repositoryMock.Verify(x => x.Delete(creditCard), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Pruebas Negativas - DeleteAsync

    [Fact]
    public async Task DeleteAsync_ConIdInexistente_DebeLanzarExcepcion()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repositoryMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CreditCardEntity?)null);

        // Act
        var act = async () => await _service.DeleteAsync(id);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Tarjeta no encontrada");
    }

    #endregion

    #region Pruebas Positivas - MakeChargeAsync

    [Fact]
    public async Task MakeChargeAsync_ConMontoValido_DebeReducirCreditoYRetornarDto()
    {
        // Arrange
        var creditCard = CreditCardEntity.Create(
            "4111111111111111", "Juan Perez", "12/25", "123", 5000m, "Visa");

        var chargeDto = new ChargeDto(1500m);

        _repositoryMock.Setup(x => x.GetByIdAsync(creditCard.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(creditCard);

        // Act
        var result = await _service.MakeChargeAsync(creditCard.Id, chargeDto);

        // Assert
        result.AvailableCredit.Should().Be(3500m);

        _repositoryMock.Verify(x => x.Update(creditCard), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Pruebas Negativas - MakeChargeAsync

    [Fact]
    public async Task MakeChargeAsync_ConIdInexistente_DebeLanzarExcepcion()
    {
        // Arrange
        var id = Guid.NewGuid();
        var chargeDto = new ChargeDto(100m);

        _repositoryMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CreditCardEntity?)null);

        // Act
        var act = async () => await _service.MakeChargeAsync(id, chargeDto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Tarjeta no encontrada");
    }

    [Fact]
    public async Task MakeChargeAsync_ConCreditoInsuficiente_DebeLanzarExcepcion()
    {
        // Arrange
        var creditCard = CreditCardEntity.Create(
            "4111111111111111", "Juan Perez", "12/25", "123", 1000m, "Visa");

        var chargeDto = new ChargeDto(1500m);

        _repositoryMock.Setup(x => x.GetByIdAsync(creditCard.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(creditCard);

        // Act
        var act = async () => await _service.MakeChargeAsync(creditCard.Id, chargeDto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Crédito insuficiente");
    }

    #endregion

    #region Pruebas Positivas - MakePaymentAsync

    [Fact]
    public async Task MakePaymentAsync_ConMontoValido_DebeAumentarCreditoYRetornarDto()
    {
        // Arrange
        var creditCard = CreditCardEntity.Create(
            "4111111111111111", "Juan Perez", "12/25", "123", 5000m, "Visa");
        creditCard.MakeCharge(2000m); // Disponible: 3000

        var paymentDto = new PaymentDto(1500m);

        _repositoryMock.Setup(x => x.GetByIdAsync(creditCard.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(creditCard);

        // Act
        var result = await _service.MakePaymentAsync(creditCard.Id, paymentDto);

        // Assert
        result.AvailableCredit.Should().Be(4500m);

        _repositoryMock.Verify(x => x.Update(creditCard), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Pruebas Negativas - MakePaymentAsync

    [Fact]
    public async Task MakePaymentAsync_ConIdInexistente_DebeLanzarExcepcion()
    {
        // Arrange
        var id = Guid.NewGuid();
        var paymentDto = new PaymentDto(100m);

        _repositoryMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CreditCardEntity?)null);

        // Act
        var act = async () => await _service.MakePaymentAsync(id, paymentDto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Tarjeta no encontrada");
    }

    [Fact]
    public async Task MakePaymentAsync_ConPagoQueExcedeLimite_DebeLanzarExcepcion()
    {
        // Arrange
        var creditCard = CreditCardEntity.Create(
            "4111111111111111", "Juan Perez", "12/25", "123", 5000m, "Visa");

        var paymentDto = new PaymentDto(100m); // Excedería el límite

        _repositoryMock.Setup(x => x.GetByIdAsync(creditCard.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(creditCard);

        // Act
        var act = async () => await _service.MakePaymentAsync(creditCard.Id, paymentDto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("El pago excede el límite de crédito");
    }

    #endregion

    #region Pruebas Positivas - DeactivateAsync

    [Fact]
    public async Task DeactivateAsync_ConIdExistente_DebeDesactivarYRetornarDto()
    {
        // Arrange
        var creditCard = CreditCardEntity.Create(
            "4111111111111111", "Juan Perez", "12/25", "123", 5000m, "Visa");

        _repositoryMock.Setup(x => x.GetByIdAsync(creditCard.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(creditCard);

        // Act
        var result = await _service.DeactivateAsync(creditCard.Id);

        // Assert
        result.IsActive.Should().BeFalse();

        _repositoryMock.Verify(x => x.Update(creditCard), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Pruebas Negativas - DeactivateAsync

    [Fact]
    public async Task DeactivateAsync_ConIdInexistente_DebeLanzarExcepcion()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repositoryMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CreditCardEntity?)null);

        // Act
        var act = async () => await _service.DeactivateAsync(id);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Tarjeta no encontrada");
    }

    #endregion

    #region Pruebas Positivas - ActivateAsync

    [Fact]
    public async Task ActivateAsync_ConIdExistente_DebeActivarYRetornarDto()
    {
        // Arrange
        var creditCard = CreditCardEntity.Create(
            "4111111111111111", "Juan Perez", "12/25", "123", 5000m, "Visa");
        creditCard.Deactivate();

        _repositoryMock.Setup(x => x.GetByIdAsync(creditCard.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(creditCard);

        // Act
        var result = await _service.ActivateAsync(creditCard.Id);

        // Assert
        result.IsActive.Should().BeTrue();

        _repositoryMock.Verify(x => x.Update(creditCard), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Pruebas Negativas - ActivateAsync

    [Fact]
    public async Task ActivateAsync_ConIdInexistente_DebeLanzarExcepcion()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repositoryMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CreditCardEntity?)null);

        // Act
        var act = async () => await _service.ActivateAsync(id);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Tarjeta no encontrada");
    }

    #endregion
}
