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

    public override GetFacilityResponse Handle(GameModel model)
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
                Type = new XrpcUByte(0),
                CompanyCode = new XrpcString("00"),
                CustomerCode = new XrpcString("0000"),
                CountryName = new XrpcString("Japan"),
                CountryJpName = new XrpcString("日本"),
                RegionName = new XrpcString("Tokyo"),
                RegionJpName = new XrpcString("東京都"),
                Accuracy = new XrpcUByte(0),
                Latitude = new XrpcInt(0),
                Longitude = new XrpcInt(0)
            },
            Line = new FacilityLine()
            {
                Id = new XrpcString("1"),
                Class = new XrpcUByte(0),
                UpClass = new XrpcUByte(0),
                Rtt = new XrpcUShort(0)
            },
            PortForward = new FacilityPortForward()
            {
                GlobalIp = new XrpcIp4("127.0.0.1"),
                GlobalPort = new XrpcUShort(5246),
                PrivatePort = new XrpcUShort(5246)
            },
            Public = new FacilityPublic()
            {
                Flag = new XrpcUByte(1),
                Name = new XrpcString("Medusa"),
                Latitude = new XrpcString("0"),
                Longitude = new XrpcString("0")
            },
            Share = new FacilityShare()
            {
                Url = new FacilityUrl()
                {
                    EaPass = new XrpcString("http://eagate.573.jp"),
                    ArcadeFan = new XrpcString("http://eagate.573.jp"),
                    KonamiNetDx = new XrpcString("http://eagate.573.jp"),
                    KonamiId = new XrpcString("http://eagate.573.jp"),
                    EaGate = new XrpcString("http://eagate.573.jp")
                },
                EaPass = new FacilityEaPass()
                {
                    Valid = new XrpcUShort(365)
                },
                EaCoin = new FacilityEaCoin()
                {
                    NotchAmount = new XrpcInt(0),
                    NotchCount = new XrpcInt(0),
                    SupplyLimit = new XrpcInt(1000000)
                }
            },
            Calendar = new FacilityCalendar()
            {
                Year = new XrpcShort((short)DateTime.UtcNow.Year),
                // TODO: no real holiday calendar wired up yet - empty list is a
                // safe default (client just sees "no holidays"), not a guess at
                // actual dates.
                Holiday = new FacilityHolidayList()
                {
                    Count = 0,
                    Value = ""
                }
            }
        };

        return facility;
    }
}
