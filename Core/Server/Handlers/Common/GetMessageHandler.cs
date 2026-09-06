using Abstractions.Handlers;
using Server.Models.Response;

namespace Server.Handlers.Common;

public class GetMessageHandler: HandlerWithoutRequest<GetMessageResponse>
{
    public override void Configure()
    {
        Module("message");
        Method("get");
    }

    public override GetMessageResponse Handle(GameModel model)
    {
        var message = new GetMessageResponse()
        {
            Expire = 300,
            Status = 0
        };
        
        return message;
    }
}