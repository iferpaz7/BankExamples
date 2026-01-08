namespace CreditCard.Api.Endpoints;

using CreditCard.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

public static class ReportEndpoints
{
    public static void MapReportEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reports")
            .WithTags("Reports")
            .WithOpenApi();

        group.MapGet("/creditcards", GetCreditCardsReport)
            .WithName("GetCreditCardsReport")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/creditcards/{id:guid}", GetCreditCardReportById)
            .WithName("GetCreditCardReportById")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/creditcards/active", GetActiveCreditCards)
            .WithName("GetActiveCreditCards")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/creditcards/high-usage", GetCreditCardsWithHighUsage)
            .WithName("GetCreditCardsWithHighUsage")
            .Produces(StatusCodes.Status200OK);
    }

    private static async Task<IResult> GetCreditCardsReport(
        [FromServices] ICreditCardReportService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetCreditCardsReportAsync(cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetCreditCardReportById(
        Guid id,
        [FromServices] ICreditCardReportService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetCreditCardReportByIdAsync(id, cancellationToken);
        return result == null ? Results.NotFound() : Results.Ok(result);
    }

    private static async Task<IResult> GetActiveCreditCards(
        [FromServices] ICreditCardReportService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetActiveCreditCardsAsync(cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetCreditCardsWithHighUsage(
        [FromQuery] decimal minPercentage,
        [FromServices] ICreditCardReportService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetCreditCardsWithHighUsageAsync(minPercentage, cancellationToken);
        return Results.Ok(result);
    }
}
