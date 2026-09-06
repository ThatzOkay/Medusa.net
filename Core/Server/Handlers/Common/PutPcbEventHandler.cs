using Abstractions.Handlers;
using Abstractions.SerializationTypes;
using Server.Models.Request;
using Server.Models.Response;

namespace Server.Handlers.Common;

public class PutPcbEventHandler : Handler<PutPcbEventRequest, PutPcbEventResponse>
{
    public override void Configure()
    {
        Module("pcbevent");
        Method("put");
    }

    public override PutPcbEventResponse Handle(PutPcbEventRequest req, GameModel model)
    {
        var pcbEvent = new PutPcbEventResponse()
        {
            Status = 0
        };
        
        return pcbEvent;
    }
}