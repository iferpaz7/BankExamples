using CreditCard.Api.Endpoints;
using CreditCard.Application;
using CreditCard.Infrastructure;
using CreditCard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Credit Card API v1");
    });
    
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<CreditCardDbContext>();
    await dbContext.Database.MigrateAsync();
}
else if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<CreditCardDbContext>();
    await dbContext.Database.MigrateAsync();
}

if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}

app.MapCreditCardEndpoints();
app.MapReportEndpoints();

app.Run();

// Para pruebas de integración
public partial class Program { }
