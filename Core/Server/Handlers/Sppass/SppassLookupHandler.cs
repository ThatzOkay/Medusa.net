using Abstractions.Handlers;
using Server.Models.Request;
using Server.Models.Response;
using Server.Services;

namespace Server.Handlers.Sppass;

public class SppassLookupHandler(ISppassSessionService sessionService) : Handler<SppassLookupRequest, SppassLookupResponse>
{
    private readonly ISppassSessionService _sessionService = sessionService;

    public override void Configure()
    {
        Module("sppass");
        Method("lookup");
    }

    public override SppassLookupResponse Handle(SppassLookupRequest req, GameModel model)
    {
        var session = _sessionService.GetSession(req.Token);

        // Unknown/expired token gets the same "no card yet" shape as a
        // genuinely-still-waiting session rather than an error: the cabinet
        // only ever polls a token it just received from open, so this only
        // happens if the session timed out mid-poll, and the cabinet already
        // handles that itself via its own expiry (expire_datetime, reported
        // on open).
        if (session is null || !session.IsApproved)
        {
            return new SppassLookupResponse
            {
                Status = 0,
                Url = SppassOpenHandler.BuildCardlessUrl(req.Token),
                Interval = 3,
                CardType = "",
                CardId = ""
            };
        }

        return new SppassLookupResponse
        {
            Status = 0,
            Url = SppassOpenHandler.BuildCardlessUrl(req.Token),
            Interval = 3,
            CardType = session.CardType!,
            CardId = session.CardId!
        };
    }
}
