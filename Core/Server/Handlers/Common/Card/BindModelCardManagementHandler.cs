using System.Xml.Linq;
using Abstractions.Handlers;
using Server.Models.Response;

namespace Server.Handlers.Common.Card;

public class BindModelCardManagementHandler : HandlerWithoutRequest<BindModelResponse>
{
    public override void Configure()
    {
        Module("cardmng");
        Method("bindmodel");
    }
    public override BindModelResponse Handle(string model)
    {
        var bindModel = new BindModelResponse()
        {
            Status = 0
        };
        return bindModel;
    }
}