namespace CreditCard.Application;

using CreditCard.Application.Interfaces;
using CreditCard.Application.Services;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICreditCardService, CreditCardService>();
        services.AddScoped<ICreditCardReportService, CreditCardReportService>();

        return services;
    }
}
