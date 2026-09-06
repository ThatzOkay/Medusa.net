using Abstractions.Handlers;
using Server.Models.Response;

namespace Server.Handlers.Common;

public class WriteEventLogHandler : HandlerWithoutRequest<WriteEventLogResponse>
{
    public override void Configure()
    {
        Module("eventlog");
        Method("write");
    }

    public override WriteEventLogResponse Handle(GameModel model)
    {
        return new WriteEventLogResponse()
        {
            Status = 0
        };
    }
}
