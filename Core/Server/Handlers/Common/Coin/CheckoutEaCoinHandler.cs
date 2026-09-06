using Abstractions.Handlers;
using Server.Models.Request;
using Server.Models.Response;
using Server.Services;

namespace Server.Handlers.Common.Coin;

public class CheckoutEaCoinHandler(IEaCoinSessionService sessionService) : Handler<EaCoinCheckoutRequest, EaCoinCheckoutResponse>
{
    public override void Configure()
    {
        Module("eacoin");
        Method("checkout");
    }

    public override EaCoinCheckoutResponse Handle(EaCoinCheckoutRequest req, GameModel model)
    {
        sessionService.RemoveSession(req.sessionId);

        return new EaCoinCheckoutResponse()
        {
            Status = 0
        };
    }
}