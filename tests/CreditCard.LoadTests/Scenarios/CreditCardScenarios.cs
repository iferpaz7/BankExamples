using System.Net.Http.Json;
using CreditCard.LoadTests.Models;
using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Http.CSharp;

namespace CreditCard.LoadTests.Scenarios;

/// <summary>
/// Escenarios de pruebas de carga para la API de Credit Card
/// </summary>
public class CreditCardScenarios
{
    private readonly string _baseUrl;
    private readonly HttpClient _httpClient;
    private static int _cardCounter = 0;
    private static readonly Random _random = new();

    public CreditCardScenarios(string baseUrl)
    {
        _baseUrl = baseUrl.TrimEnd('/');
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };
        _httpClient = new HttpClient(handler) { BaseAddress = new Uri(_baseUrl) };
    }

    /// <summary>
    /// Genera un número de tarjeta único para las pruebas
    /// </summary>
    private static string GenerateCardNumber()
    {
        var counter = Interlocked.Increment(ref _cardCounter);
        var timestamp = DateTime.Now.Ticks % 10000000;
        return $"4{counter:D6}{timestamp:D9}"[..16];
    }

    /// <summary>
    /// Genera una solicitud de creación de tarjeta
    /// </summary>
    private static CreateCreditCardRequest GenerateCreateRequest()
    {
        var cardTypes = new[] { "VISA", "MASTERCARD", "AMEX", "DISCOVER" };
        return new CreateCreditCardRequest
        {
            CardNumber = GenerateCardNumber(),
            CardHolderName = $"USUARIO CARGA {_random.Next(1000, 9999)}",
            ExpirationDate = DateTime.Now.AddYears(3).ToString("MM/yy"),
            CVV = _random.Next(100, 9999).ToString().PadLeft(3, '0')[..3],
            CreditLimit = _random.Next(1000, 50000),
            CardType = cardTypes[_random.Next(cardTypes.Length)]
        };
    }

    /// <summary>
    /// Escenario: Obtener todas las tarjetas (GET /api/creditcards)
    /// </summary>
    public ScenarioProps GetAllCardsScenario(int rate, TimeSpan duration)
    {
        return Scenario.Create("get_all_cards", async context =>
        {
            var request = Http.CreateRequest("GET", $"{_baseUrl}/api/creditcards")
                .WithHeader("Accept", "application/json");

            var response = await Http.Send(_httpClient, request);
            
            return response;
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: rate, interval: TimeSpan.FromSeconds(1), during: duration)
        );
    }

    /// <summary>
    /// Escenario: Crear tarjeta (POST /api/creditcards)
    /// </summary>
    public ScenarioProps CreateCardScenario(int rate, TimeSpan duration)
    {
        return Scenario.Create("create_card", async context =>
        {
            var createRequest = GenerateCreateRequest();
            
            var request = Http.CreateRequest("POST", $"{_baseUrl}/api/creditcards")
                .WithHeader("Content-Type", "application/json")
                .WithHeader("Accept", "application/json")
                .WithJsonBody(createRequest);

            var response = await Http.Send(_httpClient, request);
            
            return response;
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: rate, interval: TimeSpan.FromSeconds(1), during: duration)
        );
    }

    /// <summary>
    /// Escenario: Flujo completo CRUD
    /// </summary>
    public ScenarioProps FullCrudFlowScenario(int rate, TimeSpan duration)
    {
        return Scenario.Create("full_crud_flow", async context =>
        {
            try
            {
                // 1. Crear tarjeta
                var createRequest = GenerateCreateRequest();
                var createResponse = await _httpClient.PostAsJsonAsync("/api/creditcards", createRequest);
                
                if (!createResponse.IsSuccessStatusCode)
                    return Response.Fail(statusCode: (int)createResponse.StatusCode);

                var createdCard = await createResponse.Content.ReadFromJsonAsync<CreditCardResponse>();
                if (createdCard == null)
                    return Response.Fail(message: "Failed to deserialize created card");

                // 2. Obtener tarjeta por ID
                var getResponse = await _httpClient.GetAsync($"/api/creditcards/{createdCard.Id}");
                if (!getResponse.IsSuccessStatusCode)
                    return Response.Fail(statusCode: (int)getResponse.StatusCode);

                // 3. Realizar cargo
                var chargeAmount = Math.Min(100, createdCard.CreditLimit * 0.1m);
                var chargeResponse = await _httpClient.PostAsJsonAsync(
                    $"/api/creditcards/{createdCard.Id}/charge",
                    new AmountRequest { Amount = chargeAmount });
                
                if (!chargeResponse.IsSuccessStatusCode)
                    return Response.Fail(statusCode: (int)chargeResponse.StatusCode);

                // 4. Realizar pago
                var paymentResponse = await _httpClient.PostAsJsonAsync(
                    $"/api/creditcards/{createdCard.Id}/payment",
                    new AmountRequest { Amount = chargeAmount / 2 });
                
                if (!paymentResponse.IsSuccessStatusCode)
                    return Response.Fail(statusCode: (int)paymentResponse.StatusCode);

                // 5. Desactivar tarjeta
                var deactivateResponse = await _httpClient.PostAsync(
                    $"/api/creditcards/{createdCard.Id}/deactivate", null);
                
                if (!deactivateResponse.IsSuccessStatusCode)
                    return Response.Fail(statusCode: (int)deactivateResponse.StatusCode);

                // 6. Activar tarjeta
                var activateResponse = await _httpClient.PostAsync(
                    $"/api/creditcards/{createdCard.Id}/activate", null);
                
                if (!activateResponse.IsSuccessStatusCode)
                    return Response.Fail(statusCode: (int)activateResponse.StatusCode);

                // 7. Eliminar tarjeta
                var deleteResponse = await _httpClient.DeleteAsync($"/api/creditcards/{createdCard.Id}");
                
                return deleteResponse.IsSuccessStatusCode 
                    ? Response.Ok() 
                    : Response.Fail(statusCode: (int)deleteResponse.StatusCode);
            }
            catch (Exception ex)
            {
                return Response.Fail(message: ex.Message);
            }
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: rate, interval: TimeSpan.FromSeconds(1), during: duration)
        );
    }

    /// <summary>
    /// Escenario: Operaciones mixtas (lectura/escritura)
    /// </summary>
    public ScenarioProps MixedOperationsScenario(int rate, TimeSpan duration)
    {
        return Scenario.Create("mixed_operations", async context =>
        {
            var operation = _random.Next(100);
            
            try
            {
                if (operation < 60) // 60% lecturas
                {
                    var response = await _httpClient.GetAsync("/api/creditcards");
                    return response.IsSuccessStatusCode 
                        ? Response.Ok() 
                        : Response.Fail(statusCode: (int)response.StatusCode);
                }
                else if (operation < 80) // 20% creaciones
                {
                    var createRequest = GenerateCreateRequest();
                    var response = await _httpClient.PostAsJsonAsync("/api/creditcards", createRequest);
                    return response.IsSuccessStatusCode 
                        ? Response.Ok() 
                        : Response.Fail(statusCode: (int)response.StatusCode);
                }
                else // 20% operaciones en tarjetas existentes
                {
                    // Obtener tarjetas existentes
                    var getResponse = await _httpClient.GetAsync("/api/creditcards");
                    if (!getResponse.IsSuccessStatusCode)
                        return Response.Fail(statusCode: (int)getResponse.StatusCode);

                    var cards = await getResponse.Content.ReadFromJsonAsync<List<CreditCardResponse>>();
                    if (cards == null || cards.Count == 0)
                    {
                        // Si no hay tarjetas, crear una
                        var createRequest = GenerateCreateRequest();
                        var createResponse = await _httpClient.PostAsJsonAsync("/api/creditcards", createRequest);
                        return createResponse.IsSuccessStatusCode 
                            ? Response.Ok() 
                            : Response.Fail(statusCode: (int)createResponse.StatusCode);
                    }

                    var randomCard = cards[_random.Next(cards.Count)];
                    
                    if (randomCard.IsActive && randomCard.AvailableCredit > 10)
                    {
                        var chargeAmount = Math.Min(10, randomCard.AvailableCredit * 0.01m);
                        var chargeResponse = await _httpClient.PostAsJsonAsync(
                            $"/api/creditcards/{randomCard.Id}/charge",
                            new AmountRequest { Amount = chargeAmount });
                        return chargeResponse.IsSuccessStatusCode 
                            ? Response.Ok() 
                            : Response.Fail(statusCode: (int)chargeResponse.StatusCode);
                    }
                    else
                    {
                        var paymentResponse = await _httpClient.PostAsJsonAsync(
                            $"/api/creditcards/{randomCard.Id}/payment",
                            new AmountRequest { Amount = 5 });
                        return paymentResponse.IsSuccessStatusCode 
                            ? Response.Ok() 
                            : Response.Fail(statusCode: (int)paymentResponse.StatusCode);
                    }
                }
            }
            catch (Exception ex)
            {
                return Response.Fail(message: ex.Message);
            }
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: rate, interval: TimeSpan.FromSeconds(1), during: duration)
        );
    }

    /// <summary>
    /// Escenario: Prueba de estrés con carga constante
    /// </summary>
    public ScenarioProps StressTestScenario(int copies, TimeSpan duration)
    {
        return Scenario.Create("stress_test", async context =>
        {
            var createRequest = GenerateCreateRequest();
            
            var request = Http.CreateRequest("POST", $"{_baseUrl}/api/creditcards")
                .WithHeader("Content-Type", "application/json")
                .WithHeader("Accept", "application/json")
                .WithJsonBody(createRequest);

            var response = await Http.Send(_httpClient, request);
            
            return response;
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.KeepConstant(copies: copies, during: duration)
        );
    }

    /// <summary>
    /// Escenario: Prueba de picos (spike test)
    /// </summary>
    public ScenarioProps SpikeTestScenario()
    {
        return Scenario.Create("spike_test", async context =>
        {
            var response = await _httpClient.GetAsync("/api/creditcards");
            return response.IsSuccessStatusCode 
                ? Response.Ok() 
                : Response.Fail(statusCode: (int)response.StatusCode);
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            // Fase 1: Carga normal
            Simulation.Inject(rate: 10, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30)),
            // Fase 2: Pico repentino
            Simulation.Inject(rate: 200, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30)),
            // Fase 3: Vuelta a la normalidad
            Simulation.Inject(rate: 10, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30)),
            // Fase 4: Otro pico
            Simulation.Inject(rate: 300, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30)),
            // Fase 5: Recuperación
            Simulation.Inject(rate: 10, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );
    }

    /// <summary>
    /// Escenario: Prueba de reportes
    /// </summary>
    public ScenarioProps ReportsScenario(int rate, TimeSpan duration)
    {
        return Scenario.Create("reports", async context =>
        {
            try
            {
                // Obtener reporte general
                var reportResponse = await _httpClient.GetAsync("/api/reports/creditcards");
                if (!reportResponse.IsSuccessStatusCode)
                    return Response.Fail(statusCode: (int)reportResponse.StatusCode);

                // Obtener tarjetas activas
                var activeResponse = await _httpClient.GetAsync("/api/reports/creditcards/active");
                if (!activeResponse.IsSuccessStatusCode)
                    return Response.Fail(statusCode: (int)activeResponse.StatusCode);

                // Obtener tarjetas con alto uso
                var highUsageResponse = await _httpClient.GetAsync("/api/reports/creditcards/high-usage?percentage=50");
                
                return highUsageResponse.IsSuccessStatusCode 
                    ? Response.Ok() 
                    : Response.Fail(statusCode: (int)highUsageResponse.StatusCode);
            }
            catch (Exception ex)
            {
                return Response.Fail(message: ex.Message);
            }
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: rate, interval: TimeSpan.FromSeconds(1), during: duration)
        );
    }
}
