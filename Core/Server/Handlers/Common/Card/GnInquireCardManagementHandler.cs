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

        public override async Task<GnInquireResponse> HandleAsync(InquireRequest req, string model)
        {
            var splitModel = model.Split(':');
            var gameCode = splitModel[0];

            var plugin = pluginService.FindPlugin(gameCode);

            var cardId = req.CardId;

            var inquireResponse = new GnInquireResponse()
            {
                Status = 0
            };

            if (string.IsNullOrEmpty(cardId))
            {
                inquireResponse.Status = 111;
                return inquireResponse;
            }

            var konamiId = cardService.ConvertUidToKonamiId(cardId);

            if (string.IsNullOrEmpty(konamiId))
            {
                inquireResponse.Status = 111;
                return inquireResponse;
            }

            var exisitngCard = await cardService.FindByKonamiId(konamiId);

            if (exisitngCard == null)
            {
                inquireResponse.Status = 112;
                return inquireResponse;
            }

            if (plugin is null)
            {
                inquireResponse.Status = 113;
                return inquireResponse;
            }

            var profileExists = await plugin!.DoesProfileExist(cardId);

            var userIdFlag = profileExists ? "1" : "0";
            var extIdFlag = profileExists ? "1" : "0";
            var newFlag = profileExists ? "0" : "1";
            var isGeniune = profileExists ? "1" : "0";

            inquireResponse.Binded = 0;
            inquireResponse.DataId = konamiId;
            inquireResponse.EcFlag = 1;
            inquireResponse.Expired = 0;
            inquireResponse.NewFlag = newFlag;
            inquireResponse.IsGeniune = isGeniune;
            inquireResponse.ExtidFlag = extIdFlag;
            inquireResponse.RefId = konamiId;
            inquireResponse.UserIdFlag = userIdFlag;

            return inquireResponse;
        }
    }
}
