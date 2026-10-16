using Abstractions.Handlers;
using Server.Models.Response;

namespace Server.Handlers.Common.OTAUpdates;

public class DlStatusDoneHandler : HandlerWithoutRequest<DlStatusDoneResponse>
{
    public override void Configure()
    {
        Module("dlstatus");
        Method("done");
    }

    public override DlStatusDoneResponse Handle(GameModel model) =>
        new() { Status = 0, Progress = 0 };
}
