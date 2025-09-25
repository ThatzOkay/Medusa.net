using Abstractions.Handlers;
using Abstractions.SerializationTypes;
using Server.Models.Response;

namespace Server.Handlers.Common;

public class GetFacilityHandler: HandlerWithoutRequest<GetFacilityResponse>
{
    public override void Configure()
    {
        Module("facility");
        Method("get");
    }

    public override GetFacilityResponse Handle(string model)
    {
        var facility = new GetFacilityResponse()
        {
            Status = 0,
            Location = new FacilityLocation()
            {
                Id = new XrpcString("00000000"),
                Country = new XrpcString("US"),
                Region = new XrpcString("NA"),
                Name = new XrpcString("Medusa"),
                Type = new XrpcULong(0)
            },
            Line = new FacilityLine()
            {
                Id = new XrpcString("1"),
                Class = new XrpcULong(0)
            },
            PortForward = new FacilityPortForward()
            {
                GlobalIp = new XrpcString("127.0.0.1"),
                GlobalPort = new XrpcULong(5246),
                PrivatePort = new XrpcULong(5246)
            },
            Public = new FacilityPublic()
            {
                Flag = new XrpcULong(1),
                Name = new XrpcString("Medusa"),
                Latitude = new XrpcString("0.0"),
                Longitude = new XrpcString("0.0")
            },
            Share = new FacilityShare()
            {
                EaCoin = new FacilityEaCoin()
                {
                    NotchAmount = new XrpcLong(3000),
                    NotchCount = new XrpcLong(3),
                    SupplyLimit = new XrpcLong(100000)
                },
                Url = new FacilityUrl()
                {
                    EaPass = new XrpcString("http://eagate.573.jp"),
                    ArcadeFan = new XrpcString("http://eagate.573.jp"),
                    KonamiNetDx = new XrpcString("http://eagate.573.jp"),
                    KonamiId = new XrpcString("http://eagate.573.jp"),
                    EaGate = new XrpcString("http://eagate.573.jp")
                }
            }
        };
        
        return facility;
    }
}