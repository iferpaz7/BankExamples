using CreditCard.LoadTests.Scenarios;
using Microsoft.Extensions.Configuration;
using NBomber.CSharp;

namespace CreditCard.LoadTests;

/// <summary>
/// Ejecutor de pruebas de carga para Credit Card API
/// </summary>
public class LoadTestRunner
{
    private readonly IConfiguration _configuration;
    private readonly string _baseUrl;
    private readonly CreditCardScenarios _scenarios;

    public LoadTestRunner()
    {
        _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        _baseUrl = _configuration["LoadTest:BaseUrl"] ?? "https://localhost:7001";
        _scenarios = new CreditCardScenarios(_baseUrl);
    }

    /// <summary>
    /// Ejecuta prueba de humo (Smoke Test)
    /// </summary>
    public void RunSmokeTest()
    {
        Console.WriteLine("=== SMOKE TEST ===");
        Console.WriteLine($"URL Base: {_baseUrl}");
        Console.WriteLine("Validando funcionamiento básico con carga mínima...\n");

        var scenario = _scenarios.GetAllCardsScenario(
            rate: 5, 
            duration: TimeSpan.FromMinutes(1));

        NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("LoadTestResults/SmokeTest")
            .WithReportFileName($"smoke_test_{DateTime.Now:yyyyMMdd_HHmmss}")
            .WithReportFormats(
                NBomber.Contracts.ReportFormat.Html,
                NBomber.Contracts.ReportFormat.Csv,
                NBomber.Contracts.ReportFormat.Md)
            .Run();
    }

    /// <summary>
    /// Ejecuta prueba de carga normal (Load Test)
    /// </summary>
    public void RunLoadTest()
    {
        Console.WriteLine("=== LOAD TEST ===");
        Console.WriteLine($"URL Base: {_baseUrl}");
        Console.WriteLine("Simulando carga normal esperada...\n");

        var getAllScenario = _scenarios.GetAllCardsScenario(
            rate: 30, 
            duration: TimeSpan.FromMinutes(3));

        var createScenario = _scenarios.CreateCardScenario(
            rate: 10, 
            duration: TimeSpan.FromMinutes(3));

        var mixedScenario = _scenarios.MixedOperationsScenario(
            rate: 10, 
            duration: TimeSpan.FromMinutes(3));

        NBomberRunner
            .RegisterScenarios(getAllScenario, createScenario, mixedScenario)
            .WithReportFolder("LoadTestResults/LoadTest")
            .WithReportFileName($"load_test_{DateTime.Now:yyyyMMdd_HHmmss}")
            .WithReportFormats(
                NBomber.Contracts.ReportFormat.Html,
                NBomber.Contracts.ReportFormat.Csv,
                NBomber.Contracts.ReportFormat.Md)
            .Run();
    }

    /// <summary>
    /// Ejecuta prueba de estrés (Stress Test)
    /// </summary>
    public void RunStressTest()
    {
        Console.WriteLine("=== STRESS TEST ===");
        Console.WriteLine($"URL Base: {_baseUrl}");
        Console.WriteLine("Encontrando límites del sistema...\n");

        var stressScenario = _scenarios.StressTestScenario(
            copies: 100, 
            duration: TimeSpan.FromMinutes(5));

        NBomberRunner
            .RegisterScenarios(stressScenario)
            .WithReportFolder("LoadTestResults/StressTest")
            .WithReportFileName($"stress_test_{DateTime.Now:yyyyMMdd_HHmmss}")
            .WithReportFormats(
                NBomber.Contracts.ReportFormat.Html,
                NBomber.Contracts.ReportFormat.Csv,
                NBomber.Contracts.ReportFormat.Md)
            .Run();
    }

    /// <summary>
    /// Ejecuta prueba de picos (Spike Test)
    /// </summary>
    public void RunSpikeTest()
    {
        Console.WriteLine("=== SPIKE TEST ===");
        Console.WriteLine($"URL Base: {_baseUrl}");
        Console.WriteLine("Simulando picos repentinos de tráfico...\n");

        var spikeScenario = _scenarios.SpikeTestScenario();

        NBomberRunner
            .RegisterScenarios(spikeScenario)
            .WithReportFolder("LoadTestResults/SpikeTest")
            .WithReportFileName($"spike_test_{DateTime.Now:yyyyMMdd_HHmmss}")
            .WithReportFormats(
                NBomber.Contracts.ReportFormat.Html,
                NBomber.Contracts.ReportFormat.Csv,
                NBomber.Contracts.ReportFormat.Md)
            .Run();
    }

    /// <summary>
    /// Ejecuta prueba CRUD completa
    /// </summary>
    public void RunFullCrudTest()
    {
        Console.WriteLine("=== FULL CRUD FLOW TEST ===");
        Console.WriteLine($"URL Base: {_baseUrl}");
        Console.WriteLine("Ejecutando flujo completo CRUD...\n");

        var crudScenario = _scenarios.FullCrudFlowScenario(
            rate: 5, 
            duration: TimeSpan.FromMinutes(2));

        NBomberRunner
            .RegisterScenarios(crudScenario)
            .WithReportFolder("LoadTestResults/CrudTest")
            .WithReportFileName($"crud_test_{DateTime.Now:yyyyMMdd_HHmmss}")
            .WithReportFormats(
                NBomber.Contracts.ReportFormat.Html,
                NBomber.Contracts.ReportFormat.Csv,
                NBomber.Contracts.ReportFormat.Md)
            .Run();
    }

    /// <summary>
    /// Ejecuta prueba de reportes
    /// </summary>
    public void RunReportsTest()
    {
        Console.WriteLine("=== REPORTS TEST ===");
        Console.WriteLine($"URL Base: {_baseUrl}");
        Console.WriteLine("Probando endpoints de reportes...\n");

        var reportsScenario = _scenarios.ReportsScenario(
            rate: 10, 
            duration: TimeSpan.FromMinutes(2));

        NBomberRunner
            .RegisterScenarios(reportsScenario)
            .WithReportFolder("LoadTestResults/ReportsTest")
            .WithReportFileName($"reports_test_{DateTime.Now:yyyyMMdd_HHmmss}")
            .WithReportFormats(
                NBomber.Contracts.ReportFormat.Html,
                NBomber.Contracts.ReportFormat.Csv,
                NBomber.Contracts.ReportFormat.Md)
            .Run();
    }

    /// <summary>
    /// Ejecuta todas las pruebas
    /// </summary>
    public void RunAllTests()
    {
        Console.WriteLine("=== EJECUTANDO TODAS LAS PRUEBAS DE CARGA ===\n");
        
        RunSmokeTest();
        Console.WriteLine("\n--- Completado: Smoke Test ---\n");
        
        Thread.Sleep(5000); // Pausa entre pruebas
        
        RunLoadTest();
        Console.WriteLine("\n--- Completado: Load Test ---\n");
        
        Thread.Sleep(5000);
        
        RunFullCrudTest();
        Console.WriteLine("\n--- Completado: CRUD Test ---\n");
        
        Thread.Sleep(5000);
        
        RunReportsTest();
        Console.WriteLine("\n--- Completado: Reports Test ---\n");
        
        Thread.Sleep(5000);
        
        RunSpikeTest();
        Console.WriteLine("\n--- Completado: Spike Test ---\n");
        
        Thread.Sleep(5000);
        
        RunStressTest();
        Console.WriteLine("\n--- Completado: Stress Test ---\n");
        
        Console.WriteLine("=== TODAS LAS PRUEBAS COMPLETADAS ===");
        Console.WriteLine("Los reportes se encuentran en la carpeta 'LoadTestResults'");
    }
}
