using Abstractions.Services;
using Facet.Extensions;
using Microsoft.AspNetCore.Mvc;
using Server.Models.Request;
using Server.Models.Response;

namespace Server.Api;

public static class CardsApi
{
    public static RouteGroupBuilder MapCardsApiEndpoints(this RouteGroupBuilder group)
    {
        var cardsGroup = group.MapGroup("/cards").WithTags("Cards");
        cardsGroup.MapPost("/validate", ValidateCard).Produces<ValidateCardResponse>(200);
        cardsGroup.MapGet("/my", GetUserCards).Produces<CardDto[]>(200).RequireAuthorization();
        return group;
    }

    internal static async Task<IResult> ValidateCard(ValidateCardRequest request, [FromServices] ICardService cardService)
    {
        var isRegistered = await cardService.IsRegistered(request.CardId, request.Pin);
        if (isRegistered)
        {
            return Results.Ok(new ValidateCardResponse
            {
                Message = "Card already registered.",
                Success = false
            });
        }

        var exists = await cardService.Exists(request.CardId);

        if (!exists)
        {
            return Results.Ok(new ValidateCardResponse
            {
                Message = "Card does not exist.",
                Success = false
            });
        }

        var validPin = await cardService.ValidatePinAsync(request.CardId, request.Pin);

        if (validPin)
        {
            return Results.Ok(new ValidateCardResponse
            {
                Message = "Card is valid.",
                Success = true
            });
        }

        return Results.Ok(new ValidateCardResponse
        {
            Message = "Card does not exist.",
            Success = false
        });
    }

    internal static async Task<IResult> GetUserCards(HttpContext httpContext, [FromServices] ICardService cardService)
    {
        var user = httpContext.User;
        if (!(httpContext.User.Identity?.IsAuthenticated ?? false))
        {
            return Results.Unauthorized();
        }
        var userId = user.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Results.Unauthorized();
        }
        var cards = await cardService.GetCardsByUserIdAsync(int.Parse(userId));
        
        return Results.Ok(cards.SelectFacets<CardDto>());
    }
}
