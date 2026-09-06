using Abstractions.Handlers;
using Abstractions.Services;
using Server.Models.Request;
using Server.Models.Response;
using Server.Services;

namespace Server.Handlers.Common.Card
{
    public class GnInquireCardManagementHandler(IPluginService pluginService, ICardService cardService) : Handler<InquireRequest, GnInquireResponse>
    {
        public override void Configure()
        {
            Module("cardmng");
            Method("gninquire");
        }

        public override async Task<GnInquireResponse> HandleAsync(InquireRequest req, GameModel model)
        {
            var lookup = await CardInquireLookup.ResolveAsync(req.CardId, model, pluginService, cardService);

            if (lookup.ErrorStatus is { } errorStatus)
            {
                return new GnInquireResponse { Status = errorStatus };
            }

            var flag = lookup.ProfileExists ? "1" : "0";
            var newFlag = lookup.ProfileExists ? "0" : "1";

            return new GnInquireResponse
            {
                Status = 0,
                Binded = 0,
                DataId = lookup.KonamiId,
                EcFlag = 1,
                Expired = 0,
                NewFlag = newFlag,
                IsGeniune = flag,
                ExtidFlag = flag,
                RefId = lookup.KonamiId,
                UserIdFlag = flag
            };
        }
    }
}
