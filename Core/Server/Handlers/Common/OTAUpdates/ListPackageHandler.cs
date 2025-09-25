using Abstractions.Handlers;
using Server.Models.Response;

namespace Server.Handlers.Common.OTAUpdates;

public class ListPackageHandler: HandlerWithoutRequest<ListPackageResponse>
{
    public override void Configure()
    {
        Module("package");
        Method("list");
    }

    public override ListPackageResponse Handle(string model)
    {
        var package = new ListPackageResponse()
        {
            Expire = 600,
            Status = 0
        };
        
        return package;
    }
}