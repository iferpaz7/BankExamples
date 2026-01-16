namespace CreditCard.Web.Models;

/// <summary>
/// Modelo para configuración de prueba de carga
/// </summary>
public class LoadTestConfiguration
{
    public string TestType { get; set; } = "smoke";
    public int DurationSeconds { get; set; } = 60;
    public int RequestsPerSecond { get; set; } = 5;
    public int ConcurrentUsers { get; set; } = 10;
}

/// <summary>
/// Modelo para resultado de prueba de carga
/// </summary>
public class LoadTestResultModel
{
    public string ScenarioName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan Duration => EndTime - StartTime;
    public int TotalRequests { get; set; }
    public int SuccessfulRequests { get; set; }
    public int FailedRequests { get; set; }
    public double RequestsPerSecond { get; set; }
    public double AverageResponseTimeMs { get; set; }
    public double P50ResponseTimeMs { get; set; }
    public double P95ResponseTimeMs { get; set; }
    public double P99ResponseTimeMs { get; set; }
    public double ErrorRate => TotalRequests > 0 ? (double)FailedRequests / TotalRequests * 100 : 0;
    public bool Passed { get; set; }
    public List<EndpointMetric> EndpointMetrics { get; set; } = [];
}

/// <summary>
/// MMétrica por endpoint
/// </summary>
public class EndpointMetric
{
    public string Endpoint { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public int TotalRequests { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public double AverageResponseTimeMs { get; set; }
    public double MinResponseTimeMs { get; set; }
    public double MaxResponseTimeMs { get; set; }
}

/// <summary>
/// Estado de ejecución de prueba
/// </summary>
public class LoadTestStatus
{
    public bool IsRunning { get; set; }
    public string CurrentTest { get; set; } = string.Empty;
    public int Progress { get; set; }
    public int TotalRequestsSent { get; set; }
    public int SuccessfulRequests { get; set; }
    public int FailedRequests { get; set; }
    public double CurrentRps { get; set; }
    public List<string> Logs { get; set; } = [];
}

/// <summary>
/// Tipo de prueba de carga
/// </summary>
public enum LoadTestType
{
    Smoke,
    Load,
    Stress,
    Spike,
    Crud,
    Reports
}
