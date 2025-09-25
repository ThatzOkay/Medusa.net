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

    public override async Task<AuthPassResponse> HandleAsync(AuthPassRequest req, string model)
    {
        var status = 0;
        var card = await cardService.FindByKonamiId(req.ReferenceId);

        if (card is null)
        {
            status = 116;
        }

        if (card!.User.Pin != int.Parse(req.Password))
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