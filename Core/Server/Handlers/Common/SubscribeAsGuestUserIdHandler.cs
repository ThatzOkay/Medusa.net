using Abstractions.Handlers;
using Server.Models.Response;

namespace Server.Handlers.Common;

public class SubscribeAsGuestUserIdHandler : HandlerWithoutRequest<SubscribeAsGuestUserIdResponse>
{        public override void Configure()
    {
        Module("userid");
        Method("subscribeAsGuest");
    }

    public override SubscribeAsGuestUserIdResponse Handle(string model)
    {
        var subscribeAsGuest = new SubscribeAsGuestUserIdResponse()
        {
            Status = 0
        };
        return subscribeAsGuest;
    }
}