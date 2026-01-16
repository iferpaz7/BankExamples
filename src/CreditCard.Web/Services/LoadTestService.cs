using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Http.Json;
using CreditCard.Web.Models;

namespace CreditCard.Web.Services;

/// <summary>
/// Servicio para ejecutar pruebas de carga desde Blazor
/// </summary>
public class LoadTestService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<LoadTestService> _logger;
    private CancellationTokenSource? _cancellationTokenSource;
    private readonly ConcurrentQueue<string> _logs = new();

    public LoadTestStatus CurrentStatus { get; private set; } = new();
    public List<LoadTestResultModel> TestHistory { get; } = [];

    public event Action? OnStatusChanged;

    public LoadTestService(HttpClient httpClient, ILogger<LoadTestService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Ejecuta una prueba de carga
    /// </summary>
    public async Task<LoadTestResultModel> RunTestAsync(LoadTestConfiguration config)
    {
        if (CurrentStatus.IsRunning)
            throw new InvalidOperationException("Ya hay una prueba en ejecución");

        _cancellationTokenSource = new CancellationTokenSource();
        var token = _cancellationTokenSource.Token;

        CurrentStatus = new LoadTestStatus
        {
            IsRunning = true,
            CurrentTest = GetTestName(config.TestType),
            Progress = 0
        };

        AddLog($"Iniciando prueba: {CurrentStatus.CurrentTest}");
        NotifyStatusChanged();

        var result = new LoadTestResultModel
        {
            ScenarioName = CurrentStatus.CurrentTest,
            StartTime = DateTime.Now
        };

        var metrics = new ConcurrentBag<RequestMetric>();
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var tasks = new List<Task>();
            var duration = TimeSpan.FromSeconds(config.DurationSeconds);
            var endTime = DateTime.Now.Add(duration);

            AddLog($"Duración: {config.DurationSeconds}s, RPS objetivo: {config.RequestsPerSecond}");

            while (DateTime.Now < endTime && !token.IsCancellationRequested)
            {
                var batchStart = DateTime.Now;
                
                // Crear tareas para el nNúmero de requests por segundo
                for (int i = 0; i < config.RequestsPerSecond && !token.IsCancellationRequested; i++)
                {
                    var task = ExecuteRequestAsync(config.TestType, metrics, token);
                    tasks.Add(task);
                }

                // Actualizar progreso
                var elapsed = stopwatch.Elapsed;
                CurrentStatus.Progress = (int)(elapsed.TotalSeconds / config.DurationSeconds * 100);
                CurrentStatus.TotalRequestsSent = metrics.Count;
                CurrentStatus.SuccessfulRequests = metrics.Count(m => m.Success);
                CurrentStatus.FailedRequests = metrics.Count(m => !m.Success);
                CurrentStatus.CurrentRps = elapsed.TotalSeconds > 0 
                    ? metrics.Count / elapsed.TotalSeconds 
                    : 0;

                NotifyStatusChanged();

                // Esperar hasta el siguiente segundo
                var elapsed2 = DateTime.Now - batchStart;
                if (elapsed2.TotalMilliseconds < 1000)
                {
                    await Task.Delay(1000 - (int)elapsed2.TotalMilliseconds, token);
                }
            }

            // Esperar que terminen todas las tareas pendientes
            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException)
        {
            AddLog("Prueba cancelada por el usuario");
        }
        catch (Exception ex)
        {
            AddLog($"Error: {ex.Message}");
            _logger.LogError(ex, "Error en prueba de carga");
        }

        stopwatch.Stop();

        // Calcular resultados
        result.EndTime = DateTime.Now;
        result.TotalRequests = metrics.Count;
        result.SuccessfulRequests = metrics.Count(m => m.Success);
        result.FailedRequests = metrics.Count(m => !m.Success);
        result.RequestsPerSecond = stopwatch.Elapsed.TotalSeconds > 0 
            ? metrics.Count / stopwatch.Elapsed.TotalSeconds 
            : 0;

        var responseTimes = metrics.Select(m => m.ResponseTimeMs).OrderBy(t => t).ToList();
        if (responseTimes.Count != 0)
        {
            result.AverageResponseTimeMs = responseTimes.Average();
            result.P50ResponseTimeMs = GetPercentile(responseTimes, 50);
            result.P95ResponseTimeMs = GetPercentile(responseTimes, 95);
            result.P99ResponseTimeMs = GetPercentile(responseTimes, 99);
        }

        result.Passed = result.ErrorRate < 5 && result.P95ResponseTimeMs < 1000;

        // Agrupar mMétricas por endpoint
        result.EndpointMetrics = metrics
            .GroupBy(m => new { m.Endpoint, m.Method })
            .Select(g => new EndpointMetric
            {
                Endpoint = g.Key.Endpoint,
                Method = g.Key.Method,
                TotalRequests = g.Count(),
                SuccessCount = g.Count(m => m.Success),
                FailureCount = g.Count(m => !m.Success),
                AverageResponseTimeMs = g.Average(m => m.ResponseTimeMs),
                MinResponseTimeMs = g.Min(m => m.ResponseTimeMs),
                MaxResponseTimeMs = g.Max(m => m.ResponseTimeMs)
            })
            .ToList();

        CurrentStatus.IsRunning = false;
        CurrentStatus.Progress = 100;
        CurrentStatus.Logs = _logs.ToList();
        
        AddLog($"Prueba completada. Total: {result.TotalRequests}, Éxitos: {result.SuccessfulRequests}, Fallos: {result.FailedRequests}");
        AddLog($"RPS promedio: {result.RequestsPerSecond:F2}, P95: {result.P95ResponseTimeMs:F2}ms");
        
        TestHistory.Insert(0, result);
        NotifyStatusChanged();

        return result;
    }

    /// <summary>
    /// Cancela la prueba en ejecución
    /// </summary>
    public void CancelTest()
    {
        _cancellationTokenSource?.Cancel();
        AddLog("Cancelando prueba...");
        NotifyStatusChanged();
    }

    /// <summary>
    /// Limpia los logs
    /// </summary>
    public void ClearLogs()
    {
        while (_logs.TryDequeue(out _)) { }
        CurrentStatus.Logs.Clear();
        NotifyStatusChanged();
    }

    private async Task ExecuteRequestAsync(string testType, ConcurrentBag<RequestMetric> metrics, CancellationToken token)
    {
        var metric = new RequestMetric();
        var sw = Stopwatch.StartNew();

        try
        {
            switch (testType.ToLower())
            {
                case "smoke":
                case "load":
                    await ExecuteGetAllAsync(metric, token);
                    break;
                case "stress":
                case "spike":
                    await ExecuteMixedAsync(metric, token);
                    break;
                case "crud":
                    await ExecuteCrudFlowAsync(metric, token);
                    break;
                case "reports":
                    await ExecuteReportsAsync(metric, token);
                    break;
                default:
                    await ExecuteGetAllAsync(metric, token);
                    break;
            }
        }
        catch (Exception ex)
        {
            metric.Success = false;
            metric.ErrorMessage = ex.Message;
        }

        sw.Stop();
        metric.ResponseTimeMs = sw.Elapsed.TotalMilliseconds;
        metrics.Add(metric);
    }

    private async Task ExecuteGetAllAsync(RequestMetric metric, CancellationToken token)
    {
        metric.Method = "GET";
        metric.Endpoint = "/api/creditcards";
        
        var response = await _httpClient.GetAsync("api/creditcards", token);
        metric.Success = response.IsSuccessStatusCode;
        metric.StatusCode = (int)response.StatusCode;
    }

    private async Task ExecuteMixedAsync(RequestMetric metric, CancellationToken token)
    {
        var random = Random.Shared.Next(100);

        if (random < 70) // 70% GET
        {
            await ExecuteGetAllAsync(metric, token);
        }
        else // 30% POST (crear tarjeta)
        {
            metric.Method = "POST";
            metric.Endpoint = "/api/creditcards";

            var request = new
            {
                CardNumber = GenerateCardNumber(),
                CardHolderName = $"USUARIO CARGA {Random.Shared.Next(1000, 9999)}",
                ExpirationDate = "12/28",
                CVV = "123",
                CreditLimit = Random.Shared.Next(1000, 50000),
                CardType = "VISA"
            };

            var response = await _httpClient.PostAsJsonAsync("api/creditcards", request, token);
            metric.Success = response.IsSuccessStatusCode;
            metric.StatusCode = (int)response.StatusCode;
        }
    }

    private async Task ExecuteCrudFlowAsync(RequestMetric metric, CancellationToken token)
    {
        metric.Method = "CRUD";
        metric.Endpoint = "/api/creditcards (flujo)";

        // 1. Crear
        var createRequest = new
        {
            CardNumber = GenerateCardNumber(),
            CardHolderName = $"CRUD TEST {Random.Shared.Next(1000, 9999)}",
            ExpirationDate = "12/28",
            CVV = "123",
            CreditLimit = 10000m,
            CardType = "VISA"
        };

        var createResponse = await _httpClient.PostAsJsonAsync("api/creditcards", createRequest, token);
        if (!createResponse.IsSuccessStatusCode)
        {
            metric.Success = false;
            metric.StatusCode = (int)createResponse.StatusCode;
            return;
        }

        var card = await createResponse.Content.ReadFromJsonAsync<CardResponse>(cancellationToken: token);
        if (card == null)
        {
            metric.Success = false;
            return;
        }

        // 2. Leer
        var getResponse = await _httpClient.GetAsync($"api/creditcards/{card.Id}", token);
        if (!getResponse.IsSuccessStatusCode)
        {
            metric.Success = false;
            metric.StatusCode = (int)getResponse.StatusCode;
            return;
        }

        // 3. Cargo
        var chargeResponse = await _httpClient.PostAsJsonAsync(
            $"api/creditcards/{card.Id}/charge",
            new { Amount = 100m },
            token);

        // 4. Pago
        var paymentResponse = await _httpClient.PostAsJsonAsync(
            $"api/creditcards/{card.Id}/payment",
            new { Amount = 50m },
            token);

        // 5. Eliminar
        var deleteResponse = await _httpClient.DeleteAsync($"api/creditcards/{card.Id}", token);
        
        metric.Success = deleteResponse.IsSuccessStatusCode;
        metric.StatusCode = (int)deleteResponse.StatusCode;
    }

    private async Task ExecuteReportsAsync(RequestMetric metric, CancellationToken token)
    {
        metric.Method = "GET";
        metric.Endpoint = "/api/reports";

        var response = await _httpClient.GetAsync("api/reports/creditcards", token);
        metric.Success = response.IsSuccessStatusCode;
        metric.StatusCode = (int)response.StatusCode;
    }

    private static string GenerateCardNumber()
    {
        var timestamp = DateTime.Now.Ticks % 10000000000;
        var random = Random.Shared.Next(100000, 999999);
        return $"4{random}{timestamp}"[..16];
    }

    private static double GetPercentile(List<double> sortedData, int percentile)
    {
        if (sortedData.Count == 0) return 0;
        var index = (int)Math.Ceiling(percentile / 100.0 * sortedData.Count) - 1;
        return sortedData[Math.Max(0, Math.Min(index, sortedData.Count - 1))];
    }

    private string GetTestName(string testType) => testType.ToLower() switch
    {
        "smoke" => "Smoke Test",
        "load" => "Load Test",
        "stress" => "Stress Test",
        "spike" => "Spike Test",
        "crud" => "CRUD Flow Test",
        "reports" => "Reports Test",
        _ => "Load Test"
    };

    private void AddLog(string message)
    {
        var logMessage = $"[{DateTime.Now:HH:mm:ss}] {message}";
        _logs.Enqueue(logMessage);
        CurrentStatus.Logs = _logs.TakeLast(100).ToList();
        _logger.LogInformation("{Message}", message);
    }

    private void NotifyStatusChanged() => OnStatusChanged?.Invoke();

    private class RequestMetric
    {
        public string Method { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public double ResponseTimeMs { get; set; }
        public string? ErrorMessage { get; set; }
    }

    private class CardResponse
    {
        public Guid Id { get; set; }
    }
}
