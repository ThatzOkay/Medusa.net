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
                Country = new XrpcString("JP"),
                Region = new XrpcString("13"),
                Name = new XrpcString("Medusa"),
                Type = new XrpcULong(0),
                CompanyCode = new XrpcString("00"),
                CustomerCode = new XrpcString("0000"),
                CountryName = new XrpcString("Japan"),
                CountryJpName = new XrpcString("日本"),
                RegionName = new XrpcString("Tokyo"),
                RegionJpName = new XrpcString("東京都"),
                Accuracy = new XrpcULong(0),
                Latitude = new XrpcInt(0),
                Longitude = new XrpcInt(0)
            },
            Line = new FacilityLine()
            {
                Id = new XrpcString("1"),
                Class = new XrpcULong(0)
            },
            PortForward = new FacilityPortForward()
            {
                GlobalIp = new XrpcIp4("127.0.0.1"),
                GlobalPort = new XrpcULong(5246),
                PrivatePort = new XrpcULong(5246)
            },
            Public = new FacilityPublic()
            {
                Flag = new XrpcULong(1),
                Name = new XrpcString("Medusa")
            },
            Share = new FacilityShare()
            {
                EaCoin = new FacilityEaCoin()
                {
                    NotchAmount = new XrpcLong((long)3000),
                    NotchCount = new XrpcLong((long)3),
                    SupplyLimit = new XrpcLong(1000000)
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