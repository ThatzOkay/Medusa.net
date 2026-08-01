using Abstractions.Handlers;
using Abstractions.SerializationTypes;
using Abstractions.Utils;
using Server.Models.Response;

namespace Server.Handlers.Common;

public class GetFacilityHandler: HandlerWithoutRequest<GetFacilityResponse>
{
    private static readonly string ListeningAddress = IpUtils.GetLocalIPv4() ?? "";

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
                Region = new XrpcString("JP-13"),
                CustomerCode = new XrpcString("X000000001"),
                CompanyCode = new XrpcString("X000000001"),
                Latitude = new XrpcLong((long)0),
                Longitude = new XrpcLong((long)0),
                Accuracy = new XrpcULong(0),
                CountryName = new XrpcString("Japan"),
                RegionName = new XrpcString("Tokyo"),
                CountryJName = new XrpcString("日本国"),
                RegionJName = new XrpcString("東京都"),
                Name = new XrpcString("Medusa"),
                Type = new XrpcULong(255)
            },
            Line = new FacilityLine()
            {
                Class = new XrpcULong(8),
                RTT = new XrpcULong(500),
                UpClass = new XrpcULong(8),
                Id = new XrpcString("1")
            },
            PortForward = new FacilityPortForward()
            {
                GlobalIp = new XrpcIp4(ListeningAddress),
                GlobalPort = new XrpcULong(5246),
                PrivatePort = new XrpcULong(5246)
            },
            Public = new FacilityPublic()
            {
                Flag = new XrpcULong(1),
                Name = new XrpcString("Medusa"),
                Latitude = new XrpcString("0"),
                Longitude = new XrpcString("0")
            },
            Share = new FacilityShare()
            {
                EaCoin = new FacilityEaCoin()
                {
                    NotchAmount = new XrpcLong((long)3000),
                    NotchCount = new XrpcLong((long)3),
                    SupplyLimit = new XrpcLong(9999)
                },
                EaPass = new FacilityEaPass()
                {
                    Valid = new XrpcULong(365)
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