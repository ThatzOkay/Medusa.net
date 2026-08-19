using Abstractions.Entities;
using Abstractions.Handlers;
using Abstractions.Services;
using Microsoft.AspNetCore.Identity;
using Server.Models.Request;
using Server.Models.Response;

namespace Server.Handlers.Common.Card;

public class GetRefIdCardManagementHandler(ICardService cardService, UserManager<User> userManager) : Handler<GetRefIdRequest, GetRefIdResponse>
{

    public override void Configure()
    {
        Module("cardmng");
        Method("getrefid");
    }

    public override async Task<GetRefIdResponse> HandleAsync(GetRefIdRequest req, string model)
    {
        var konamiId = cardService.ConvertUidToKonamiId(req.CardId);
        
        var card = new Abstractions.Entities.Card { RawId = req.CardId, KonamiId = konamiId };
        var user = new User { Pin = req.Password, UserName = konamiId, Cards = [card] };
        
        await userManager.CreateAsync(user);

        var getRefId = new GetRefIdResponse()
        {
            Status = 0,
            DataId = card.RawId,
            ReferenceId = card.Id.ToString().PadLeft(16, '0')
        };
        
        return getRefId;
    }
}