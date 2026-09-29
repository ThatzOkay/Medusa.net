using Abstractions.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Server.Models.Response;
using Server.Services;

namespace Server.Api;

public static class CardlessApi
{
    public static RouteGroupBuilder MapCardlessApiEndpoints(this RouteGroupBuilder group)
    {
        var cardlessGroup = group.MapGroup("/cardless").WithTags("Cardless");
        cardlessGroup.MapPost("/{token}/approve", ApproveSession).RequireAuthorization();
        return group;
    }

    // The phone side of cardless login never picks a card - it approves
    // whichever token the cabinet's QR encodes using the signed-in player's
    // first registered card. sppass.lookup's response only ever carries a
    // single card_type/card_id pair per token - there's no field anywhere in
    // the protocol for the cabinet to receive (or the phone to submit) a
    // choice among several cards, so this is a deliberate simplification
    private static async Task<Results<Ok<ApproveSessionResponse>, UnauthorizedHttpResult, BadRequest<ApproveSessionResponse>, NotFound<ApproveSessionResponse>>> ApproveSession(
        string token, HttpContext httpContext,
        [FromServices] ICardService cardService, [FromServices] ISppassSessionService sessionService)
    {
        var user = httpContext.User;
        if (!(user.Identity?.IsAuthenticated ?? false))
        {
            return TypedResults.Unauthorized();
        }

        var userIdClaim = user.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return TypedResults.Unauthorized();
        }

        var cards = await cardService.GetCardsByUserIdAsync(long.Parse(userIdClaim));
        var card = cards.FirstOrDefault();

        if (card is null)
        {
            return TypedResults.BadRequest(new ApproveSessionResponse { Success = false, Message = "No card registered on this account yet." });
        }

        // Confirmed against decompiled Assembly-CSharp: CardEntryScene casts
        // sppass.lookup's card_type straight into (MCARD=0,
        // ICCARD=1, FELICA=2, VIRTUAL=4, CARDLESS=5) and sets Account.IsCardless=true
        var approved = sessionService.TryApprove(token, "5", card.RawId);

        if (!approved)
        {
            return TypedResults.NotFound(new ApproveSessionResponse { Success = false, Message = "This QR code has expired - go back to the cabinet and try again." });
        }

        return TypedResults.Ok(new ApproveSessionResponse { Success = true, Message = $"Approved with card {card.KonamiId}." });
    }
}
