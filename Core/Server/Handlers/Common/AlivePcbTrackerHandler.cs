using Abstractions.Handlers;
using Server.Models.Response;

namespace Server.Handlers.Common;

public class AlivePcbTrackerHandler : HandlerWithoutRequest<AlivePcbTrackerResponse>
{
    public override void Configure()
    {
        Module("pcbtracker");
        Method("alive");
    }

    public override AlivePcbTrackerResponse Handle(GameModel model)
    {
        var pcbtracker = new AlivePcbTrackerResponse()
        {
            Status = 0,
            Expire = 1200,
            EcEnable = 1,
            EcLimit = 0,
            Limit = 0,
            Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
        
        return pcbtracker;
    }
}