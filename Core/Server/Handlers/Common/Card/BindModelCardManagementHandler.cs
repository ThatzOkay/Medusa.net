using System.Xml.Linq;
using Abstractions.Handlers;
using Server.Models.Request;
using Server.Models.Response;

namespace Server.Handlers.Common.Card;

public class BindModelCardManagementHandler : Handler<BindModelRequest, BindModelResponse>
{
    public override void Configure()
    {
        Module("cardmng");
        Method("bindmodel");
    }
    public override BindModelResponse Handle(BindModelRequest req, string model)
    {
        var bindModel = new BindModelResponse()
        {
            Status = 0,
            DataId = req.ReferenceId
        };
        return bindModel;
    }
}