using Abstractions.Handlers;
using Server.Models.Request;
using Server.Models.Response;
using Server.Services;

namespace Server.Handlers.Common.Coin;

public class EaCoinCheckoutHandler(IEaCoinSessionService sessionService) : Handler<EaCoinCheckoutRequest, EaCoinCheckoutResponse>
{
    public override void Configure()
    {
        Module("eacoin");
        Method("checkout");
    }

    public override EaCoinCheckoutResponse Handle(EaCoinCheckoutRequest req, string model)
    {
        sessionService.RemoveSession(req.sessionId);

        return new EaCoinCheckoutResponse()
        {
            Status = 0
        };
    }
}