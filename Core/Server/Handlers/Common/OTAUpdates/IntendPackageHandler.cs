using Abstractions.Handlers;
using Server.Models.Response;

namespace Server.Handlers.Common.OTAUpdates;

public class IntendPackageHandler : HandlerWithoutRequest<IntendPackageResponse>
{
    public override void Configure()
    {
        Module("package");
        Method("intend");
    }

    public override IntendPackageResponse Handle(GameModel model) => new() { Status = 0 };
}
