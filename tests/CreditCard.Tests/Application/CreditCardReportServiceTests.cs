namespace CreditCard.Tests.Application;

using CreditCard.Application.DTOs;
using CreditCard.Application.Services;
using CreditCard.Domain.Repositories;
using FluentAssertions;
using Moq;
using Xunit;

public class CreditCardReportServiceTests
{
    private readonly Mock<ICreditCardReadRepository> _readRepositoryMock;
    private readonly CreditCardReportService _service;

    public CreditCardReportServiceTests()
    {
        _readRepositoryMock = new Mock<ICreditCardReadRepository>();
        _service = new CreditCardReportService(_readRepositoryMock.Object);
    }

    #region Pruebas Positivas - GetCreditCardsReportAsync

    [Fact]
    public async Task GetCreditCardsReportAsync_ConTarjetasExistentes_DebeRetornarListaDeReportes()
    {
        // Arrange
        var reportes = new List<CreditCardReportDto>
        {
            new(Guid.NewGuid(), "4111111111111111", "JUAN PEREZ", "Visa", 5000m, 3000m, 2000m, 40m, true),
            new(Guid.NewGuid(), "5500000000000004", "MARIA GARCIA", "MasterCard", 8000m, 6000m, 2000m, 25m, true)
        };

        _readRepositoryMock.Setup(x => x.QueryAsync<CreditCardReportDto>(
            It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reportes);

        // Act
        var result = await _service.GetCreditCardsReportAsync();

        // Assert
        result.Should().HaveCount(2);
        result.First().CardHolderName.Should().Be("JUAN PEREZ");
        result.Last().CardHolderName.Should().Be("MARIA GARCIA");
    }

    [Fact]
    public async Task GetCreditCardsReportAsync_SinTarjetas_DebeRetornarListaVacia()
    {
        // Arrange
        _readRepositoryMock.Setup(x => x.QueryAsync<CreditCardReportDto>(
            It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CreditCardReportDto>());

        // Act
        var result = await _service.GetCreditCardsReportAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region Pruebas Positivas - GetCreditCardReportByIdAsync

    [Fact]
    public async Task GetCreditCardReportByIdAsync_ConIdExistente_DebeRetornarReporte()
    {
        // Arrange
        var id = Guid.NewGuid();
        var reporte = new CreditCardReportDto(
            id, "4111111111111111", "JUAN PEREZ", "Visa", 5000m, 3000m, 2000m, 40m, true);

        _readRepositoryMock.Setup(x => x.QueryFirstOrDefaultAsync<CreditCardReportDto>(
            It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reporte);

        // Act
        var result = await _service.GetCreditCardReportByIdAsync(id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        result.CardHolderName.Should().Be("JUAN PEREZ");
        result.UsedCredit.Should().Be(2000m);
        result.UsagePercentage.Should().Be(40m);
    }

    #endregion

    #region Pruebas Negativas - GetCreditCardReportByIdAsync

    [Fact]
    public async Task GetCreditCardReportByIdAsync_ConIdInexistente_DebeRetornarNull()
    {
        // Arrange
        var id = Guid.NewGuid();

        _readRepositoryMock.Setup(x => x.QueryFirstOrDefaultAsync<CreditCardReportDto>(
            It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CreditCardReportDto?)null);

        // Act
        var result = await _service.GetCreditCardReportByIdAsync(id);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Pruebas Positivas - GetActiveCreditCardsAsync

    [Fact]
    public async Task GetActiveCreditCardsAsync_ConTarjetasActivas_DebeRetornarSoloActivas()
    {
        // Arrange
        var reportesActivos = new List<CreditCardReportDto>
        {
            new(Guid.NewGuid(), "4111111111111111", "JUAN PEREZ", "Visa", 5000m, 3000m, 2000m, 40m, true),
            new(Guid.NewGuid(), "5500000000000004", "MARIA GARCIA", "MasterCard", 8000m, 6000m, 2000m, 25m, true)
        };

        _readRepositoryMock.Setup(x => x.QueryAsync<CreditCardReportDto>(
            It.Is<string>(s => s.Contains("IsActive = 1")), It.IsAny<object?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reportesActivos);

        // Act
        var result = await _service.GetActiveCreditCardsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.All(r => r.IsActive).Should().BeTrue();
    }

    [Fact]
    public async Task GetActiveCreditCardsAsync_SinTarjetasActivas_DebeRetornarListaVacia()
    {
        // Arrange
        _readRepositoryMock.Setup(x => x.QueryAsync<CreditCardReportDto>(
            It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CreditCardReportDto>());

        // Act
        var result = await _service.GetActiveCreditCardsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region Pruebas Positivas - GetCreditCardsWithHighUsageAsync

    [Fact]
    public async Task GetCreditCardsWithHighUsageAsync_ConTarjetasAltoUso_DebeRetornarFiltradas()
    {
        // Arrange
        var minPercentage = 50m;
        var reportesAltoUso = new List<CreditCardReportDto>
        {
            new(Guid.NewGuid(), "4111111111111111", "JUAN PEREZ", "Visa", 5000m, 1000m, 4000m, 80m, true),
            new(Guid.NewGuid(), "5500000000000004", "MARIA GARCIA", "MasterCard", 8000m, 2000m, 6000m, 75m, true)
        };

        _readRepositoryMock.Setup(x => x.QueryAsync<CreditCardReportDto>(
            It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reportesAltoUso);

        // Act
        var result = await _service.GetCreditCardsWithHighUsageAsync(minPercentage);

        // Assert
        result.Should().HaveCount(2);
        result.All(r => r.UsagePercentage >= minPercentage).Should().BeTrue();
    }

    [Fact]
    public async Task GetCreditCardsWithHighUsageAsync_SinTarjetasAltoUso_DebeRetornarListaVacia()
    {
        // Arrange
        var minPercentage = 90m;

        _readRepositoryMock.Setup(x => x.QueryAsync<CreditCardReportDto>(
            It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CreditCardReportDto>());

        // Act
        var result = await _service.GetCreditCardsWithHighUsageAsync(minPercentage);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCreditCardsWithHighUsageAsync_ConPorcentajeCero_DebeRetornarTodas()
    {
        // Arrange
        var minPercentage = 0m;
        var todosLosReportes = new List<CreditCardReportDto>
        {
            new(Guid.NewGuid(), "4111111111111111", "JUAN PEREZ", "Visa", 5000m, 5000m, 0m, 0m, true),
            new(Guid.NewGuid(), "5500000000000004", "MARIA GARCIA", "MasterCard", 8000m, 2000m, 6000m, 75m, true)
        };

        _readRepositoryMock.Setup(x => x.QueryAsync<CreditCardReportDto>(
            It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(todosLosReportes);

        // Act
        var result = await _service.GetCreditCardsWithHighUsageAsync(minPercentage);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetCreditCardsWithHighUsageAsync_ConPorcentaje100_DebeRetornarSoloMaximoUso()
    {
        // Arrange
        var minPercentage = 100m;
        var reporteMaximoUso = new List<CreditCardReportDto>
        {
            new(Guid.NewGuid(), "4111111111111111", "JUAN PEREZ", "Visa", 5000m, 0m, 5000m, 100m, true)
        };

        _readRepositoryMock.Setup(x => x.QueryAsync<CreditCardReportDto>(
            It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reporteMaximoUso);

        // Act
        var result = await _service.GetCreditCardsWithHighUsageAsync(minPercentage);

        // Assert
        result.Should().HaveCount(1);
        result.First().UsagePercentage.Should().Be(100m);
    }

    #endregion
}
