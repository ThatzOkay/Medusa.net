using Abstractions.Handlers;
using Abstractions.SerializationTypes;
using Abstractions.Services;
using Server.Models.Request;
using Server.Models.Response;
using Server.Services;

namespace Server.Handlers.Common.Coin;

public class EaCoinCheckinHandler(ICardService cardService, IEaCoinSessionService sessionService) : Handler<EaCoinCheckInRequest, EaCoinCheckInResponse>
{
    public override void Configure()
    {
        Module("eacoin");
        Method("checkin");
    }

    public override async Task<EaCoinCheckInResponse> HandleAsync(EaCoinCheckInRequest req, string model)
    {
        var card = await cardService.FindByCardId(req.CardId);

        if (card is null)
        {
            return new EaCoinCheckInResponse
            {
                Expire = 1200,
                Status = 1,
                AcStatus = new XrpcUByte(0)
            };
        }

        var pinValid = await cardService.ValidatePinAsync(card.KonamiId, req.PassWd);

        if (!pinValid)
        {
            return new EaCoinCheckInResponse
            {
                Expire = 1200,
                Status = 1,
                AcStatus = new XrpcUByte(0)
            };
        }

        var session = sessionService.AddSession(card.User.Id, card.KonamiId);

        return new EaCoinCheckInResponse
        {
            Expire = 1200,
            Status = 0,
            Sequence = 0,
            AcStatus = 0,
            AcId = card.User.Id + "-" + card.KonamiId,
            AcName = card.User.UserName ?? card.KonamiId,
            Balance = card.User.PaseliAmount,
            SessionId = session.SessionId,
            InShopCharge = 0,
            Point = 0
        };
    }
}
