namespace CreditCard.IntegrationTests.Api;

using System.Net;
using System.Net.Http.Json;
using CreditCard.Application.DTOs;
using CreditCard.IntegrationTests.Fixtures;
using FluentAssertions;
using Xunit;

public class CreditCardEndpointsTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public CreditCardEndpointsTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    #region Pruebas de Integración - POST /api/creditcards

    [Fact]
    public async Task CreateCreditCard_ConDatosValidos_DebeRetornar201Created()
    {
        // Arrange
        var dto = new CreateCreditCardDto(
            $"411111111111{Random.Shared.Next(1000, 9999)}",
            "Juan Perez",
            "12/25",
            "123",
            5000m,
            "Visa");

        // Act
        var response = await _client.PostAsJsonAsync("/api/creditcards", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<CreditCardResponseDto>();
        result.Should().NotBeNull();
        result!.CardHolderName.Should().Be("JUAN PEREZ");
        result.CreditLimit.Should().Be(5000m);
        result.AvailableCredit.Should().Be(5000m);
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateCreditCard_ConNumeroTarjetaDuplicado_DebeRetornar400BadRequest()
    {
        // Arrange
        var cardNumber = $"522222222222{Random.Shared.Next(1000, 9999)}";
        var dto1 = new CreateCreditCardDto(cardNumber, "Juan Perez", "12/25", "123", 5000m, "Visa");
        var dto2 = new CreateCreditCardDto(cardNumber, "Maria Garcia", "06/26", "456", 8000m, "MasterCard");

        // Act
        await _client.PostAsJsonAsync("/api/creditcards", dto1);
        var response = await _client.PostAsJsonAsync("/api/creditcards", dto2);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateCreditCard_ConDatosInvalidos_DebeRetornar400BadRequest()
    {
        // Arrange - Número de tarjeta muy corto
        var dto = new CreateCreditCardDto("123", "Juan", "12/25", "123", 5000m, "Visa");

        // Act
        var response = await _client.PostAsJsonAsync("/api/creditcards", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Pruebas de Integración - GET /api/creditcards

    [Fact]
    public async Task GetAllCreditCards_DebeRetornar200OK()
    {
        // Act
        var response = await _client.GetAsync("/api/creditcards?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAllCreditCards_ConTarjetasCreadas_DebeRetornarResultadoPaginado()
    {
        // Arrange
        var dto = new CreateCreditCardDto(
            $"411111111111{Random.Shared.Next(1000, 9999)}",
            "Test User",
            "12/25",
            "123",
            5000m,
            "Visa");
        await _client.PostAsJsonAsync("/api/creditcards", dto);

        // Act
        var response = await _client.GetAsync("/api/creditcards?pageNumber=1&pageSize=10");
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<CreditCardResponseDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Items.Should().NotBeNull();
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetAllCreditCards_ConPaginacion_DebeRetornarPaginaCorrecta()
    {
        // Arrange - Create multiple cards
        for (int i = 0; i < 5; i++)
        {
            var dto = new CreateCreditCardDto(
                $"411111111111{Random.Shared.Next(1000, 9999)}",
                $"Test User {i}",
                "12/25",
                "123",
                5000m,
                "Visa");
            await _client.PostAsJsonAsync("/api/creditcards", dto);
        }

        // Act
        var response = await _client.GetAsync("/api/creditcards?pageNumber=1&pageSize=2");
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<CreditCardResponseDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Items.Count().Should().BeLessOrEqualTo(2);
        result.PageSize.Should().Be(2);
    }

    [Fact]
    public async Task GetAllCreditCards_SinParametros_DebeUsarValoresPorDefecto()
    {
        // Act
        var response = await _client.GetAsync("/api/creditcards?pageNumber=0&pageSize=0");
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<CreditCardResponseDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    #endregion

    #region Pruebas de Integración - GET /api/creditcards/{id}

    [Fact]
    public async Task GetCreditCardById_ConIdExistente_DebeRetornar200OK()
    {
        // Arrange
        var createDto = new CreateCreditCardDto(
            $"411111111111{Random.Shared.Next(1000, 9999)}",
            "Juan Perez",
            "12/25",
            "123",
            5000m,
            "Visa");
        var createResponse = await _client.PostAsJsonAsync("/api/creditcards", createDto);
        var createdCard = await createResponse.Content.ReadFromJsonAsync<CreditCardResponseDto>();

        // Act
        var response = await _client.GetAsync($"/api/creditcards/{createdCard!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<CreditCardResponseDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(createdCard.Id);
    }

    [Fact]
    public async Task GetCreditCardById_ConIdInexistente_DebeRetornar404NotFound()
    {
        // Act
        var response = await _client.GetAsync($"/api/creditcards/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Pruebas de Integración - PUT /api/creditcards/{id}

    [Fact]
    public async Task UpdateCreditCard_ConDatosValidos_DebeRetornar200OK()
    {
        // Arrange
        var createDto = new CreateCreditCardDto(
            $"411111111111{Random.Shared.Next(1000, 9999)}",
            "Juan Perez",
            "12/25",
            "123",
            5000m,
            "Visa");
        var createResponse = await _client.PostAsJsonAsync("/api/creditcards", createDto);
        var createdCard = await createResponse.Content.ReadFromJsonAsync<CreditCardResponseDto>();

        var updateDto = new UpdateCreditCardDto("Maria Garcia", 8000m);

        // Act
        var response = await _client.PutAsJsonAsync($"/api/creditcards/{createdCard!.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<CreditCardResponseDto>();
        result!.CardHolderName.Should().Be("MARIA GARCIA");
        result.CreditLimit.Should().Be(8000m);
    }

    [Fact]
    public async Task UpdateCreditCard_ConIdInexistente_DebeRetornar400BadRequest()
    {
        // Arrange
        var updateDto = new UpdateCreditCardDto("Maria Garcia", 8000m);

        // Act
        var response = await _client.PutAsJsonAsync($"/api/creditcards/{Guid.NewGuid()}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Pruebas de Integración - DELETE /api/creditcards/{id}

    [Fact]
    public async Task DeleteCreditCard_ConIdExistente_DebeRetornar204NoContent()
    {
        // Arrange
        var createDto = new CreateCreditCardDto(
            $"411111111111{Random.Shared.Next(1000, 9999)}",
            "Juan Perez",
            "12/25",
            "123",
            5000m,
            "Visa");
        var createResponse = await _client.PostAsJsonAsync("/api/creditcards", createDto);
        var createdCard = await createResponse.Content.ReadFromJsonAsync<CreditCardResponseDto>();

        // Act
        var response = await _client.DeleteAsync($"/api/creditcards/{createdCard!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verificar que ya no existe
        var getResponse = await _client.GetAsync($"/api/creditcards/{createdCard.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCreditCard_ConIdInexistente_DebeRetornar400BadRequest()
    {
        // Act
        var response = await _client.DeleteAsync($"/api/creditcards/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Pruebas de Integración - POST /api/creditcards/{id}/charge

    [Fact]
    public async Task MakeCharge_ConMontoValido_DebeReducirCreditoDisponible()
    {
        // Arrange
        var createDto = new CreateCreditCardDto(
            $"411111111111{Random.Shared.Next(1000, 9999)}",
            "Juan Perez",
            "12/25",
            "123",
            5000m,
            "Visa");
        var createResponse = await _client.PostAsJsonAsync("/api/creditcards", createDto);
        var createdCard = await createResponse.Content.ReadFromJsonAsync<CreditCardResponseDto>();

        var chargeDto = new ChargeDto(1500m);

        // Act
        var response = await _client.PostAsJsonAsync($"/api/creditcards/{createdCard!.Id}/charge", chargeDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<CreditCardResponseDto>();
        result!.AvailableCredit.Should().Be(3500m);
    }

    [Fact]
    public async Task MakeCharge_ConCreditoInsuficiente_DebeRetornar400BadRequest()
    {
        // Arrange
        var createDto = new CreateCreditCardDto(
            $"411111111111{Random.Shared.Next(1000, 9999)}",
            "Juan Perez",
            "12/25",
            "123",
            1000m,
            "Visa");
        var createResponse = await _client.PostAsJsonAsync("/api/creditcards", createDto);
        var createdCard = await createResponse.Content.ReadFromJsonAsync<CreditCardResponseDto>();

        var chargeDto = new ChargeDto(1500m);

        // Act
        var response = await _client.PostAsJsonAsync($"/api/creditcards/{createdCard!.Id}/charge", chargeDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task MakeCharge_ConTarjetaInexistente_DebeRetornar400BadRequest()
    {
        // Arrange
        var chargeDto = new ChargeDto(100m);

        // Act
        var response = await _client.PostAsJsonAsync($"/api/creditcards/{Guid.NewGuid()}/charge", chargeDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Pruebas de Integración - POST /api/creditcards/{id}/payment

    [Fact]
    public async Task MakePayment_ConMontoValido_DebeAumentarCreditoDisponible()
    {
        // Arrange
        var createDto = new CreateCreditCardDto(
            $"411111111111{Random.Shared.Next(1000, 9999)}",
            "Juan Perez",
            "12/25",
            "123",
            5000m,
            "Visa");
        var createResponse = await _client.PostAsJsonAsync("/api/creditcards", createDto);
        var createdCard = await createResponse.Content.ReadFromJsonAsync<CreditCardResponseDto>();

        // Primero hacer un cargo
        var chargeDto = new ChargeDto(2000m);
        await _client.PostAsJsonAsync($"/api/creditcards/{createdCard!.Id}/charge", chargeDto);

        // Luego hacer un pago
        var paymentDto = new PaymentDto(1500m);

        // Act
        var response = await _client.PostAsJsonAsync($"/api/creditcards/{createdCard.Id}/payment", paymentDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<CreditCardResponseDto>();
        result!.AvailableCredit.Should().Be(4500m); // 5000 - 2000 + 1500
    }

    [Fact]
    public async Task MakePayment_ConPagoQueExcedeLimite_DebeRetornar400BadRequest()
    {
        // Arrange
        var createDto = new CreateCreditCardDto(
            $"411111111111{Random.Shared.Next(1000, 9999)}",
            "Juan Perez",
            "12/25",
            "123",
            5000m,
            "Visa");
        var createResponse = await _client.PostAsJsonAsync("/api/creditcards", createDto);
        var createdCard = await createResponse.Content.ReadFromJsonAsync<CreditCardResponseDto>();

        var paymentDto = new PaymentDto(100m); // Excedería el límite

        // Act
        var response = await _client.PostAsJsonAsync($"/api/creditcards/{createdCard!.Id}/payment", paymentDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Pruebas de Integración - POST /api/creditcards/{id}/deactivate

    [Fact]
    public async Task DeactivateCreditCard_ConIdExistente_DebeDesactivarTarjeta()
    {
        // Arrange
        var createDto = new CreateCreditCardDto(
            $"411111111111{Random.Shared.Next(1000, 9999)}",
            "Juan Perez",
            "12/25",
            "123",
            5000m,
            "Visa");
        var createResponse = await _client.PostAsJsonAsync("/api/creditcards", createDto);
        var createdCard = await createResponse.Content.ReadFromJsonAsync<CreditCardResponseDto>();

        // Act
        var response = await _client.PostAsync($"/api/creditcards/{createdCard!.Id}/deactivate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<CreditCardResponseDto>();
        result!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeactivateCreditCard_ConIdInexistente_DebeRetornar400BadRequest()
    {
        // Act
        var response = await _client.PostAsync($"/api/creditcards/{Guid.NewGuid()}/deactivate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Pruebas de Integración - POST /api/creditcards/{id}/activate

    [Fact]
    public async Task ActivateCreditCard_ConTarjetaDesactivada_DebeActivarTarjeta()
    {
        // Arrange
        var createDto = new CreateCreditCardDto(
            $"411111111111{Random.Shared.Next(1000, 9999)}",
            "Juan Perez",
            "12/25",
            "123",
            5000m,
            "Visa");
        var createResponse = await _client.PostAsJsonAsync("/api/creditcards", createDto);
        var createdCard = await createResponse.Content.ReadFromJsonAsync<CreditCardResponseDto>();

        // Primero desactivar
        await _client.PostAsync($"/api/creditcards/{createdCard!.Id}/deactivate", null);

        // Act
        var response = await _client.PostAsync($"/api/creditcards/{createdCard.Id}/activate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<CreditCardResponseDto>();
        result!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task ActivateCreditCard_ConIdInexistente_DebeRetornar400BadRequest()
    {
        // Act
        var response = await _client.PostAsync($"/api/creditcards/{Guid.NewGuid()}/activate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Pruebas de Integración - Flujo Completo

    [Fact]
    public async Task FlujoCompleto_CRUD_DebeCompletarseExitosamente()
    {
        // 1. Crear tarjeta
        var createDto = new CreateCreditCardDto(
            $"411111111111{Random.Shared.Next(1000, 9999)}",
            "Juan Perez",
            "12/25",
            "123",
            10000m,
            "Visa");
        var createResponse = await _client.PostAsJsonAsync("/api/creditcards", createDto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var card = await createResponse.Content.ReadFromJsonAsync<CreditCardResponseDto>();

        // 2. Leer tarjeta
        var getResponse = await _client.GetAsync($"/api/creditcards/{card!.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 3. Actualizar tarjeta
        var updateDto = new UpdateCreditCardDto("Juan Perez Garcia", 15000m);
        var updateResponse = await _client.PutAsJsonAsync($"/api/creditcards/{card.Id}", updateDto);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 4. Hacer cargo
        var chargeDto = new ChargeDto(5000m);
        var chargeResponse = await _client.PostAsJsonAsync($"/api/creditcards/{card.Id}/charge", chargeDto);
        chargeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var afterCharge = await chargeResponse.Content.ReadFromJsonAsync<CreditCardResponseDto>();
        afterCharge!.AvailableCredit.Should().Be(10000m); // 15000 - 5000

        // 5. Hacer pago
        var paymentDto = new PaymentDto(3000m);
        var paymentResponse = await _client.PostAsJsonAsync($"/api/creditcards/{card.Id}/payment", paymentDto);
        paymentResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var afterPayment = await paymentResponse.Content.ReadFromJsonAsync<CreditCardResponseDto>();
        afterPayment!.AvailableCredit.Should().Be(13000m); // 10000 + 3000

        // 6. Desactivar
        var deactivateResponse = await _client.PostAsync($"/api/creditcards/{card.Id}/deactivate", null);
        deactivateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 7. Activar
        var activateResponse = await _client.PostAsync($"/api/creditcards/{card.Id}/activate", null);
        activateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 8. Eliminar
        var deleteResponse = await _client.DeleteAsync($"/api/creditcards/{card.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task FlujoCompleto_MultiplesCargosYPagos_DebeCalcularCorrectamente()
    {
        // Arrange
        var createDto = new CreateCreditCardDto(
            $"411111111111{Random.Shared.Next(1000, 9999)}",
            "Juan Perez",
            "12/25",
            "123",
            5000m,
            "Visa");
        var createResponse = await _client.PostAsJsonAsync("/api/creditcards", createDto);
        var card = await createResponse.Content.ReadFromJsonAsync<CreditCardResponseDto>();

        // Act - Múltiples cargos
        await _client.PostAsJsonAsync($"/api/creditcards/{card!.Id}/charge", new ChargeDto(1000m));
        await _client.PostAsJsonAsync($"/api/creditcards/{card.Id}/charge", new ChargeDto(500m));
        await _client.PostAsJsonAsync($"/api/creditcards/{card.Id}/charge", new ChargeDto(1500m));

        // Assert después de cargos
        var afterCharges = await _client.GetAsync($"/api/creditcards/{card.Id}");
        var cardAfterCharges = await afterCharges.Content.ReadFromJsonAsync<CreditCardResponseDto>();
        cardAfterCharges!.AvailableCredit.Should().Be(2000m); // 5000 - 3000

        // Act - Múltiples pagos
        await _client.PostAsJsonAsync($"/api/creditcards/{card.Id}/payment", new PaymentDto(1000m));
        await _client.PostAsJsonAsync($"/api/creditcards/{card.Id}/payment", new PaymentDto(500m));

        // Assert después de pagos
        var afterPayments = await _client.GetAsync($"/api/creditcards/{card.Id}");
        var cardAfterPayments = await afterPayments.Content.ReadFromJsonAsync<CreditCardResponseDto>();
        cardAfterPayments!.AvailableCredit.Should().Be(3500m); // 2000 + 1500
    }

    #endregion
}
