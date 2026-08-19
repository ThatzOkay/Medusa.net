using System.Security.Claims;
using Abstractions.Entities;
using HotChocolate.Authorization;

namespace Server.GraphQL;

[ExtendObjectType(OperationTypeNames.Query)]
public class CardQuery
{
    [UsePaging(IncludeTotalCount = true), UseFiltering, UseSorting, Authorize]
    public IQueryable<Card> GetMyCards([Service] AppDbContext appDbContext, ClaimsPrincipal claimsPrincipal, CancellationToken cancellationToken)
    {
        var parsed = long.TryParse(claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier) ?? "", out var userId);

        if (!parsed && userId is 0)
        {
            throw new ArgumentException("Cannot find userId");
        }
        
        var cards = appDbContext.Cards.Where(c => c.UserId == userId);
        return cards;
    }
}