using CreditCard.LoadTests;

Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("═       CREDIT CARD API - PRUEBAS DE CARGA CON NBOMBER         ═");
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine();

var runner = new LoadTestRunner();

Console.WriteLine("Seleccione el tipo de prueba a ejecutar:");
Console.WriteLine();
Console.WriteLine("  1. Smoke Test      - Prueba básica (1 min, 5 req/s)");
Console.WriteLine("  2. Load Test       - Carga normal (3 min, 50 req/s)");
Console.WriteLine("  3. Stress Test     - Prueba de estrés (5 min, 100 usuarios)");
Console.WriteLine("  4. Spike Test      - Picos de tráfico (2.5 min, hasta 300 req/s)");
Console.WriteLine("  5. CRUD Flow Test  - Flujo completo CRUD (2 min, 5 req/s)");
Console.WriteLine("  6. Reports Test    - Endpoints de reportes (2 min, 10 req/s)");
Console.WriteLine("  7. Ejecutar TODAS  - Todas las pruebas secuencialmente");
Console.WriteLine("  0. Salir");
Console.WriteLine();

while (true)
{
    Console.Write("Opción: ");
    var input = Console.ReadLine();

    Console.WriteLine();

    switch (input)
    {
        case "1":
            runner.RunSmokeTest();
            break;
        case "2":
            runner.RunLoadTest();
            break;
        case "3":
            runner.RunStressTest();
            break;
        case "4":
            runner.RunSpikeTest();
            break;
        case "5":
            runner.RunFullCrudTest();
            break;
        case "6":
            runner.RunReportsTest();
            break;
        case "7":
            runner.RunAllTests();
            break;
        case "0":
            Console.WriteLine("¡Hasta luego!");
            return;
        default:
            Console.WriteLine("Opción no válida. Por favor, seleccione una opción del 0 al 7.");
            continue;
    }

    Console.WriteLine();
    Console.WriteLine("Prueba completada. Los reportes están en 'LoadTestResults/'");
    Console.WriteLine();
    Console.WriteLine("Presione cualquier tecla para continuar o 0 para salir...");
    
    if (Console.ReadKey().KeyChar == '0')
    {
        Console.WriteLine("\n¡Hasta luego!");
        return;
    }
    
    Console.WriteLine();
}
