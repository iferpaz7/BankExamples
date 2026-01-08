namespace CreditCard.Api.Endpoints;

using CreditCard.Application.DTOs;
using CreditCard.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

public static class CreditCardEndpoints
{
    public static void MapCreditCardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/creditcards")
            .WithTags("Credit Cards")
            .WithOpenApi();

        group.MapPost("/", CreateCreditCard)
            .WithName("CreateCreditCard")
            .Produces<CreditCardResponseDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapGet("/", GetAllCreditCards)
            .WithName("GetAllCreditCards")
            .Produces<PagedResultDto<CreditCardResponseDto>>();

        group.MapGet("/{id:guid}", GetCreditCardById)
            .WithName("GetCreditCardById")
            .Produces<CreditCardResponseDto>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}", UpdateCreditCard)
            .WithName("UpdateCreditCard")
            .Produces<CreditCardResponseDto>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeleteCreditCard)
            .WithName("DeleteCreditCard")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/charge", MakeCharge)
            .WithName("MakeCharge")
            .Produces<CreditCardResponseDto>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/payment", MakePayment)
            .WithName("MakePayment")
            .Produces<CreditCardResponseDto>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/deactivate", DeactivateCreditCard)
            .WithName("DeactivateCreditCard")
            .Produces<CreditCardResponseDto>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/activate", ActivateCreditCard)
            .WithName("ActivateCreditCard")
            .Produces<CreditCardResponseDto>()
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> CreateCreditCard(
        [FromBody] CreateCreditCardDto dto,
        [FromServices] ICreditCardService service,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.CreateAsync(dto, cancellationToken);
            return Results.Created($"/api/creditcards/{result.Id}", result);
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> GetAllCreditCards(
        [FromQuery] int pageNumber,
        [FromQuery] int pageSize,
        [FromServices] ICreditCardService service,
        CancellationToken cancellationToken)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var result = await service.GetAllPagedAsync(pageNumber, pageSize, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetCreditCardById(
        Guid id,
        [FromServices] ICreditCardService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetByIdAsync(id, cancellationToken);
        return result == null ? Results.NotFound() : Results.Ok(result);
    }

    private static async Task<IResult> UpdateCreditCard(
        Guid id,
        [FromBody] UpdateCreditCardDto dto,
        [FromServices] ICreditCardService service,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.UpdateAsync(id, dto, cancellationToken);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> DeleteCreditCard(
        Guid id,
        [FromServices] ICreditCardService service,
        CancellationToken cancellationToken)
    {
        try
        {
            await service.DeleteAsync(id, cancellationToken);
            return Results.NoContent();
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> MakeCharge(
        Guid id,
        [FromBody] ChargeDto dto,
        [FromServices] ICreditCardService service,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.MakeChargeAsync(id, dto, cancellationToken);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> MakePayment(
        Guid id,
        [FromBody] PaymentDto dto,
        [FromServices] ICreditCardService service,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.MakePaymentAsync(id, dto, cancellationToken);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> DeactivateCreditCard(
        Guid id,
        [FromServices] ICreditCardService service,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.DeactivateAsync(id, cancellationToken);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ActivateCreditCard(
        Guid id,
        [FromServices] ICreditCardService service,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.ActivateAsync(id, cancellationToken);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }
}
