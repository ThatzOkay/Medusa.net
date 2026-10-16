using Abstractions.Handlers;
using Server.Models.Response;

namespace Server.Handlers.Common.OTAUpdates;

public class DlStatusProgressHandler : HandlerWithoutRequest<DlStatusProgressResponse>
{
    public override void Configure()
    {
        Module("dlstatus");
        Method("progress");
    }

    public override DlStatusProgressResponse Handle(GameModel model) =>
        new() { Status = 0 };
}
