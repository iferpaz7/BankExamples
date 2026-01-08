namespace CreditCard.LoadTests.Models;

/// <summary>
/// DTO para crear tarjeta de crédito en pruebas de carga
/// </summary>
public class CreateCreditCardRequest
{
    public string CardNumber { get; set; } = string.Empty;
    public string CardHolderName { get; set; } = string.Empty;
    public string ExpirationDate { get; set; } = string.Empty;
    public string CVV { get; set; } = string.Empty;
    public decimal CreditLimit { get; set; }
    public string CardType { get; set; } = string.Empty;
}

/// <summary>
/// DTO para actualizar tarjeta de crédito
/// </summary>
public class UpdateCreditCardRequest
{
    public string CardHolderName { get; set; } = string.Empty;
    public decimal CreditLimit { get; set; }
}

/// <summary>
/// DTO para cargo o pago
/// </summary>
public class AmountRequest
{
    public decimal Amount { get; set; }
}

/// <summary>
/// Respuesta de tarjeta de crédito
/// </summary>
public class CreditCardResponse
{
    public Guid Id { get; set; }
    public string CardNumber { get; set; } = string.Empty;
    public string CardHolderName { get; set; } = string.Empty;
    public string ExpirationDate { get; set; } = string.Empty;
    public string CardType { get; set; } = string.Empty;
    public decimal CreditLimit { get; set; }
    public decimal AvailableCredit { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Resultado de prueba de carga
/// </summary>
public class LoadTestResult
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
    public double P75ResponseTimeMs { get; set; }
    public double P95ResponseTimeMs { get; set; }
    public double P99ResponseTimeMs { get; set; }
    public double ErrorRate => TotalRequests > 0 ? (double)FailedRequests / TotalRequests * 100 : 0;
    public bool Passed { get; set; }
    public string ReportPath { get; set; } = string.Empty;
}
