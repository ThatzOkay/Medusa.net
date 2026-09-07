using Abstractions.Services;
using Server.Models.Request;
using Server.Models.Response;

namespace Server.Handlers.Common.Card;

using Abstractions.Handlers;

public class AuthPassCardManegementHandler(ICardService cardService) : Handler<AuthPassRequest, AuthPassResponse>
{

    public override void Configure()
    {
        Module("cardmng");
        Method("authpass");
    }

    public override async Task<AuthPassResponse> HandleAsync(AuthPassRequest req, GameModel model)
    {
        var status = 0;

        var id = req.ReferenceId.TrimStart();

        var parsed = long.TryParse(id, out var parsedId);

        Abstractions.Entities.Card? card;

        if (!parsed)
        {
            card = await cardService.FindByKonamiId(req.ReferenceId);
        }
        else
        {
            card = await cardService.FindById(parsedId);
        }

        if (card is null)
        {
            status = 116;
        }

        if (card?.User.Pin != req.Password)
        {
            status = 116;
        }

        var authPass = new AuthPassResponse()
        {
            Status = status
        };

        return authPass;
    }
}
