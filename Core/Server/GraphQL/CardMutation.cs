using System.Security.Claims;
using Abstractions.Entities;
using Abstractions.Services;
using HotChocolate.Authorization;

namespace Server.GraphQL;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class CardMutation
{
    [Authorize]
    public async Task<IQueryable<Card>> AddCard([Service] ICardService cardService, [Service] AppDbContext appDbContext, string cardNumber, ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken)
    {
        var parsed = int.TryParse(claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier) ?? "", out var userId);

        if (!parsed && userId is 0)
        {
            throw new ArgumentException("Cannot find userId");
        }

        var existingCard = await cardService.FindByKonamiId(cardNumber);

        if (existingCard is not null)
        {
            throw new Exception($"Card {cardNumber} is already exists");
        }

        var cardId = cardService.ConvertKonamiIdToUid(cardNumber);

        var card = new Card()
        {
            KonamiId = cardNumber,
            RawId = cardId,
            UserId = userId
        };

        var entity = (await appDbContext.Cards.AddAsync(card, cancellationToken)).Entity;

        await appDbContext.SaveChangesAsync(cancellationToken);
        
        return appDbContext.Cards.Where(c => c.Id == entity.Id);
    }
}