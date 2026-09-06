using Abstractions.Handlers;
using Abstractions.Services;
using Server.Models.Request;
using Server.Models.Response;
using Server.Services;

namespace Server.Handlers.Common.Card;

public class InquireCardManagementHandler(IPluginService pluginService, ICardService cardService) : Handler<InquireRequest, InquireResponse>
{
    public override void Configure()
    {
        Module("cardmng");
        Method("inquire");
    }

    public override async Task<InquireResponse> HandleAsync(InquireRequest req, GameModel model)
    {
        var lookup = await CardInquireLookup.ResolveAsync(req.CardId, model, pluginService, cardService);

        if (lookup.ErrorStatus is { } errorStatus)
        {
            return new InquireResponse { Status = errorStatus };
        }

        var flag = lookup.ProfileExists ? "1" : "0";
        var newFlag = lookup.ProfileExists ? "0" : "1";

        return new InquireResponse
        {
            Status = 0,
            Binded = 0,
            DataId = lookup.KonamiId,
            EcFlag = 1,
            Expired = 0,
            NewFlag = newFlag,
            ExtidFlag = flag,
            RefId = lookup.ExistingCard!.Id.ToString().PadLeft(16, '0'),
            UserIdFlag = flag,
            Pcode = lookup.KonamiId
        };
    }
}