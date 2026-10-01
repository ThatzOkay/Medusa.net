using Abstractions.Handlers;
using Server.Models.Response;

namespace Server.Handlers.Common;

public class KeepAlivePcbTrackerHandler : HandlerWithoutRequest<KeepAlivePcbTrackerResponse>
{
    public override void Configure()
    {
        Module("pcbtracker");
        Method("keepalive");
    }

    public override KeepAlivePcbTrackerResponse Handle(GameModel model)
    {
        var pcbEvent = new KeepAlivePcbTrackerResponse()
        {
            Status = 0
        };

        return pcbEvent;
    }
}
