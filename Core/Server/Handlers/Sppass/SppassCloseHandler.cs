using Abstractions.Handlers;
using Server.Models.Request;
using Server.Models.Response;
using Server.Services;

namespace Server.Handlers.Sppass;

public class SppassCloseHandler(ISppassSessionService sessionService) : Handler<SppassCloseRequest, SppassCloseResponse>
{
    private readonly ISppassSessionService _sessionService = sessionService;

    public override void Configure()
    {
        Module("sppass");
        Method("close");
    }

    public override SppassCloseResponse Handle(SppassCloseRequest req, GameModel model)
    {
        _sessionService.Close(req.Token);
        return new SppassCloseResponse { Status = 0 };
    }
}
