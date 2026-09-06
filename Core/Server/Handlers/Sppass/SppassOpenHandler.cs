using Abstractions.Handlers;
using Server.Models.Response;
using Server.Services;
using Server.Utils;

namespace Server.Handlers.Sppass;

public class SppassOpenHandler(ISppassSessionService sessionService) : HandlerWithoutRequest<SppassOpenResponse>
{
    private readonly ISppassSessionService _sessionService = sessionService;

    public override void Configure()
    {
        Module("sppass");
        Method("open");
    }

    public override SppassOpenResponse Handle(GameModel model)
    {
        var session = _sessionService.Open();

        return new SppassOpenResponse
        {
            Status = 0,
            Token = session.Token,
            ExpireDatetime = session.ExpiresAt.ToString("yyyy-MM-dd HH:mm:ss"),
            Url = BuildCardlessUrl(session.Token),
            // Seconds - the client turns this into ms itself, confirmed from
            // the decompiled receiver: ms = max(1000, seconds*1000), and it
            // defaults to 10s if this is omitted/zero.
            Interval = 3
        };
    }

    internal static string BuildCardlessUrl(string token)
        => $"{ServerAddress.PublicUrl.TrimEnd('/')}/cardless/{token}";
}
