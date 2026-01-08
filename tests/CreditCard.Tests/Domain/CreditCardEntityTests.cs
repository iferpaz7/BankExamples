namespace CreditCard.Tests.Domain;

using CreditCard.Domain.Entities;
using FluentAssertions;
using Xunit;

public class CreditCardEntityTests
{
    #region Pruebas Positivas - Create

    [Fact]
    public void Create_ConDatosValidos_DebeCrearTarjeta()
    {
        // Arrange
        var cardNumber = "4111111111111111";
        var cardHolderName = "Juan Perez";
        var expirationDate = "12/25";
        var cvv = "123";
        var creditLimit = 5000m;
        var cardType = "Visa";

        // Act
        var creditCard = CreditCardEntity.Create(
            cardNumber, cardHolderName, expirationDate, cvv, creditLimit, cardType);

        // Assert
        creditCard.Should().NotBeNull();
        creditCard.Id.Should().NotBeEmpty();
        creditCard.CardNumber.Should().Be(cardNumber);
        creditCard.CardHolderName.Should().Be("JUAN PEREZ");
        creditCard.ExpirationDate.Should().Be(expirationDate);
        creditCard.CVV.Should().Be(cvv);
        creditCard.CreditLimit.Should().Be(creditLimit);
        creditCard.AvailableCredit.Should().Be(creditLimit);
        creditCard.CardType.Should().Be(cardType);
        creditCard.IsActive.Should().BeTrue();
        creditCard.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        creditCard.UpdatedAt.Should().BeNull();
    }

    [Theory]
    [InlineData("1234567890123")]     // 13 dígitos - mínimo válido
    [InlineData("1234567890123456789")] // 19 dígitos - máximo válido
    public void Create_ConNumeroTarjetaValido_DebeCrearTarjeta(string cardNumber)
    {
        // Act
        var creditCard = CreditCardEntity.Create(
            cardNumber, "Juan Perez", "12/25", "123", 5000m, "Visa");

        // Assert
        creditCard.CardNumber.Should().Be(cardNumber);
    }

    [Theory]
    [InlineData("123")]  // 3 dígitos
    [InlineData("1234")] // 4 dígitos
    public void Create_ConCVVValido_DebeCrearTarjeta(string cvv)
    {
        // Act
        var creditCard = CreditCardEntity.Create(
            "4111111111111111", "Juan Perez", "12/25", cvv, 5000m, "Visa");

        // Assert
        creditCard.CVV.Should().Be(cvv);
    }

    #endregion

    #region Pruebas Negativas - Create

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ConNumeroTarjetaNuloOVacio_DebeLanzarExcepcion(string? cardNumber)
    {
        // Act
        var act = () => CreditCardEntity.Create(
            cardNumber!, "Juan Perez", "12/25", "123", 5000m, "Visa");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("El número de tarjeta es requerido");
    }

    [Theory]
    [InlineData("123456789012")]    // 12 dígitos - muy corto
    [InlineData("12345678901234567890")] // 20 dígitos - muy largo
    public void Create_ConNumeroTarjetaLongitudInvalida_DebeLanzarExcepcion(string cardNumber)
    {
        // Act
        var act = () => CreditCardEntity.Create(
            cardNumber, "Juan Perez", "12/25", "123", 5000m, "Visa");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("El número de tarjeta debe tener entre 13 y 19 dígitos");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ConNombreTitularNuloOVacio_DebeLanzarExcepcion(string? cardHolderName)
    {
        // Act
        var act = () => CreditCardEntity.Create(
            "4111111111111111", cardHolderName!, "12/25", "123", 5000m, "Visa");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("El nombre del titular es requerido");
    }

    [Theory]
    [InlineData("AB")]
    [InlineData("X")]
    public void Create_ConNombreTitularMuyCorto_DebeLanzarExcepcion(string cardHolderName)
    {
        // Act
        var act = () => CreditCardEntity.Create(
            "4111111111111111", cardHolderName, "12/25", "123", 5000m, "Visa");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("El nombre debe tener al menos 3 caracteres");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ConCVVNuloOVacio_DebeLanzarExcepcion(string? cvv)
    {
        // Act
        var act = () => CreditCardEntity.Create(
            "4111111111111111", "Juan Perez", "12/25", cvv!, 5000m, "Visa");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("El CVV es requerido");
    }

    [Theory]
    [InlineData("12")]    // 2 dígitos - muy corto
    [InlineData("12345")] // 5 dígitos - muy largo
    public void Create_ConCVVLongitudInvalida_DebeLanzarExcepcion(string cvv)
    {
        // Act
        var act = () => CreditCardEntity.Create(
            "4111111111111111", "Juan Perez", "12/25", cvv, 5000m, "Visa");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("El CVV debe tener 3 o 4 dígitos");
    }

    #endregion

    #region Pruebas Positivas - UpdateCardHolder

    [Fact]
    public void UpdateCardHolder_ConNombreValido_DebeActualizarNombre()
    {
        // Arrange
        var creditCard = CrearTarjetaValida();
        var nuevoNombre = "Maria Garcia";

        // Act
        creditCard.UpdateCardHolder(nuevoNombre);

        // Assert
        creditCard.CardHolderName.Should().Be("MARIA GARCIA");
        creditCard.UpdatedAt.Should().NotBeNull();
        creditCard.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    #endregion

    #region Pruebas Negativas - UpdateCardHolder

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateCardHolder_ConNombreNuloOVacio_DebeLanzarExcepcion(string? nuevoNombre)
    {
        // Arrange
        var creditCard = CrearTarjetaValida();

        // Act
        var act = () => creditCard.UpdateCardHolder(nuevoNombre!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("El nombre del titular es requerido");
    }

    [Fact]
    public void UpdateCardHolder_ConNombreMuyCorto_DebeLanzarExcepcion()
    {
        // Arrange
        var creditCard = CrearTarjetaValida();

        // Act
        var act = () => creditCard.UpdateCardHolder("AB");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("El nombre debe tener al menos 3 caracteres");
    }

    #endregion

    #region Pruebas Positivas - UpdateCreditLimit

    [Fact]
    public void UpdateCreditLimit_ConLimiteMayor_DebeAumentarCreditoDisponible()
    {
        // Arrange
        var creditCard = CrearTarjetaValida(); // Límite inicial: 5000
        var nuevoLimite = 8000m;

        // Act
        creditCard.UpdateCreditLimit(nuevoLimite);

        // Assert
        creditCard.CreditLimit.Should().Be(8000m);
        creditCard.AvailableCredit.Should().Be(8000m);
        creditCard.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateCreditLimit_ConLimiteMenor_DebeReducirCreditoDisponible()
    {
        // Arrange
        var creditCard = CrearTarjetaValida(); // Límite inicial: 5000
        var nuevoLimite = 3000m;

        // Act
        creditCard.UpdateCreditLimit(nuevoLimite);

        // Assert
        creditCard.CreditLimit.Should().Be(3000m);
        creditCard.AvailableCredit.Should().Be(3000m);
    }

    #endregion

    #region Pruebas Negativas - UpdateCreditLimit

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    [InlineData(-1)]
    public void UpdateCreditLimit_ConLimiteCeroONegativo_DebeLanzarExcepcion(decimal nuevoLimite)
    {
        // Arrange
        var creditCard = CrearTarjetaValida();

        // Act
        var act = () => creditCard.UpdateCreditLimit(nuevoLimite);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("El límite de crédito debe ser mayor a 0");
    }

    #endregion

    #region Pruebas Positivas - MakeCharge

    [Fact]
    public void MakeCharge_ConMontoValido_DebeReducirCreditoDisponible()
    {
        // Arrange
        var creditCard = CrearTarjetaValida(); // Crédito disponible: 5000
        var monto = 1500m;

        // Act
        creditCard.MakeCharge(monto);

        // Assert
        creditCard.AvailableCredit.Should().Be(3500m);
        creditCard.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void MakeCharge_ConMontoIgualACreditoDisponible_DebeDejarCreditoEnCero()
    {
        // Arrange
        var creditCard = CrearTarjetaValida(); // Crédito disponible: 5000

        // Act
        creditCard.MakeCharge(5000m);

        // Assert
        creditCard.AvailableCredit.Should().Be(0m);
    }

    #endregion

    #region Pruebas Negativas - MakeCharge

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    [InlineData(-1)]
    public void MakeCharge_ConMontoCeroONegativo_DebeLanzarExcepcion(decimal monto)
    {
        // Arrange
        var creditCard = CrearTarjetaValida();

        // Act
        var act = () => creditCard.MakeCharge(monto);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("El monto debe ser mayor a 0");
    }

    [Fact]
    public void MakeCharge_ConTarjetaInactiva_DebeLanzarExcepcion()
    {
        // Arrange
        var creditCard = CrearTarjetaValida();
        creditCard.Deactivate();

        // Act
        var act = () => creditCard.MakeCharge(100m);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("La tarjeta está inactiva");
    }

    [Fact]
    public void MakeCharge_ConMontoMayorACreditoDisponible_DebeLanzarExcepcion()
    {
        // Arrange
        var creditCard = CrearTarjetaValida(); // Crédito disponible: 5000

        // Act
        var act = () => creditCard.MakeCharge(5001m);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Crédito insuficiente");
    }

    #endregion

    #region Pruebas Positivas - MakePayment

    [Fact]
    public void MakePayment_ConMontoValido_DebeAumentarCreditoDisponible()
    {
        // Arrange
        var creditCard = CrearTarjetaValida();
        creditCard.MakeCharge(2000m); // Crédito disponible: 3000

        // Act
        creditCard.MakePayment(1500m);

        // Assert
        creditCard.AvailableCredit.Should().Be(4500m);
        creditCard.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void MakePayment_ConPagoTotal_DebeRestaurarCreditoCompleto()
    {
        // Arrange
        var creditCard = CrearTarjetaValida();
        creditCard.MakeCharge(5000m); // Crédito disponible: 0

        // Act
        creditCard.MakePayment(5000m);

        // Assert
        creditCard.AvailableCredit.Should().Be(5000m);
    }

    #endregion

    #region Pruebas Negativas - MakePayment

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    [InlineData(-1)]
    public void MakePayment_ConMontoCeroONegativo_DebeLanzarExcepcion(decimal monto)
    {
        // Arrange
        var creditCard = CrearTarjetaValida();
        creditCard.MakeCharge(1000m);

        // Act
        var act = () => creditCard.MakePayment(monto);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("El monto debe ser mayor a 0");
    }

    [Fact]
    public void MakePayment_ConPagoQueExcedeLimite_DebeLanzarExcepcion()
    {
        // Arrange
        var creditCard = CrearTarjetaValida(); // Límite: 5000, Disponible: 5000

        // Act
        var act = () => creditCard.MakePayment(100m); // Excedería el límite

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("El pago excede el límite de crédito");
    }

    #endregion

    #region Pruebas Positivas - Activate/Deactivate

    [Fact]
    public void Deactivate_DebeDesactivarTarjeta()
    {
        // Arrange
        var creditCard = CrearTarjetaValida();

        // Act
        creditCard.Deactivate();

        // Assert
        creditCard.IsActive.Should().BeFalse();
        creditCard.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Activate_DebeActivarTarjeta()
    {
        // Arrange
        var creditCard = CrearTarjetaValida();
        creditCard.Deactivate();

        // Act
        creditCard.Activate();

        // Assert
        creditCard.IsActive.Should().BeTrue();
        creditCard.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Deactivate_TarjetaYaInactiva_DebePermaneceInactiva()
    {
        // Arrange
        var creditCard = CrearTarjetaValida();
        creditCard.Deactivate();
        var primeraActualizacion = creditCard.UpdatedAt;

        // Act
        creditCard.Deactivate();

        // Assert
        creditCard.IsActive.Should().BeFalse();
        creditCard.UpdatedAt.Should().BeOnOrAfter(primeraActualizacion!.Value);
    }

    [Fact]
    public void Activate_TarjetaYaActiva_DebePermaneceActiva()
    {
        // Arrange
        var creditCard = CrearTarjetaValida();

        // Act
        creditCard.Activate();

        // Assert
        creditCard.IsActive.Should().BeTrue();
        creditCard.UpdatedAt.Should().NotBeNull();
    }

    #endregion

    #region Pruebas de Casos Edge - Operaciones Múltiples

    [Fact]
    public void MakeCharge_MultiplesCargos_DebeAcumularCorrectamente()
    {
        // Arrange
        var creditCard = CrearTarjetaValida(); // Crédito: 5000

        // Act
        creditCard.MakeCharge(1000m);
        creditCard.MakeCharge(500m);
        creditCard.MakeCharge(1500m);

        // Assert
        creditCard.AvailableCredit.Should().Be(2000m);
    }

    [Fact]
    public void MakePayment_MultiplesPagos_DebeAcumularCorrectamente()
    {
        // Arrange
        var creditCard = CrearTarjetaValida();
        creditCard.MakeCharge(4000m); // Disponible: 1000

        // Act
        creditCard.MakePayment(1000m);
        creditCard.MakePayment(500m);
        creditCard.MakePayment(1500m);

        // Assert
        creditCard.AvailableCredit.Should().Be(4000m);
    }

    [Fact]
    public void CicloCompleto_CargosYPagos_DebeCalcularCorrectamente()
    {
        // Arrange
        var creditCard = CrearTarjetaValida(); // Límite: 5000

        // Act - Ciclo de cargos y pagos
        creditCard.MakeCharge(2000m);  // Disponible: 3000
        creditCard.MakePayment(500m);  // Disponible: 3500
        creditCard.MakeCharge(1500m);  // Disponible: 2000
        creditCard.MakePayment(3000m); // Disponible: 5000

        // Assert
        creditCard.AvailableCredit.Should().Be(5000m);
        creditCard.CreditLimit.Should().Be(5000m);
    }

    [Fact]
    public void UpdateCreditLimit_ConDeudaExistente_DebeAjustarCreditoDisponible()
    {
        // Arrange
        var creditCard = CrearTarjetaValida(); // Límite: 5000, Disponible: 5000
        creditCard.MakeCharge(2000m); // Disponible: 3000, Deuda: 2000

        // Act - Aumentar límite
        creditCard.UpdateCreditLimit(8000m);

        // Assert - El crédito disponible debe aumentar manteniendo la deuda
        creditCard.CreditLimit.Should().Be(8000m);
        creditCard.AvailableCredit.Should().Be(6000m); // 8000 - 2000 deuda
    }

    [Fact]
    public void Create_ConNombreEnMinusculas_DebeConvertirAMayusculas()
    {
        // Act
        var creditCard = CreditCardEntity.Create(
            "4111111111111111", "juan perez garcia", "12/25", "123", 5000m, "Visa");

        // Assert
        creditCard.CardHolderName.Should().Be("JUAN PEREZ GARCIA");
    }

    [Fact]
    public void Create_ConNombreMixto_DebeConvertirAMayusculas()
    {
        // Act
        var creditCard = CreditCardEntity.Create(
            "4111111111111111", "JuAn PeReZ", "12/25", "123", 5000m, "Visa");

        // Assert
        creditCard.CardHolderName.Should().Be("JUAN PEREZ");
    }

    [Theory]
    [InlineData("Visa")]
    [InlineData("MasterCard")]
    [InlineData("American Express")]
    [InlineData("Discover")]
    public void Create_ConDiferentesTiposTarjeta_DebeCrearCorrectamente(string cardType)
    {
        // Act
        var creditCard = CreditCardEntity.Create(
            "4111111111111111", "Juan Perez", "12/25", "123", 5000m, cardType);

        // Assert
        creditCard.CardType.Should().Be(cardType);
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(1)]
    [InlineData(999999.99)]
    public void Create_ConDiferentesLimites_DebeCrearCorrectamente(decimal creditLimit)
    {
        // Act
        var creditCard = CreditCardEntity.Create(
            "4111111111111111", "Juan Perez", "12/25", "123", creditLimit, "Visa");

        // Assert
        creditCard.CreditLimit.Should().Be(creditLimit);
        creditCard.AvailableCredit.Should().Be(creditLimit);
    }

    #endregion

    #region Métodos Auxiliares

    private static CreditCardEntity CrearTarjetaValida()
    {
        return CreditCardEntity.Create(
            "4111111111111111",
            "Juan Perez",
            "12/25",
            "123",
            5000m,
            "Visa");
    }

    #endregion
}
