using System.Xml.Serialization;
using Abstractions.SerializationTypes;

namespace Server.Models.Response;

[XmlRoot("facility")]
[Serializable]
public class GetFacilityResponse
{
    [XmlAttribute("status")]
    public required int Status { get; set; }

    [XmlElement("location")]
    public required FacilityLocation Location { get; set; }

    [XmlElement("line")]
    public required FacilityLine Line { get; set; }

    [XmlElement("portfw")]
    public required FacilityPortForward PortForward { get; set; }

    [XmlElement("public")]
    public required FacilityPublic Public { get; set; }

    [XmlElement("share")]
    public required FacilityShare Share { get; set; }

    [XmlElement("calendar")]
    public required FacilityCalendar Calendar { get; set; }
}

public record FacilityLocation
{
    [XmlElement("id")]
    public required XrpcString Id { get; set; }

    [XmlElement("country")]
    public required XrpcString Country { get; set; }

    [XmlElement("region")]
    public required XrpcString Region { get; set; }

    [XmlElement("name")]
    public required XrpcString Name { get; set; }

    [XmlElement("type")]
    public required XrpcUByte Type { get; set; }

    [XmlElement("companycode")]
    public required XrpcString CompanyCode { get; set; }

    [XmlElement("customercode")]
    public required XrpcString CustomerCode { get; set; }

    [XmlElement("countryname")]
    public required XrpcString CountryName { get; set; }

    [XmlElement("countryjname")]
    public required XrpcString CountryJpName { get; set; }

    [XmlElement("regionname")]
    public required XrpcString RegionName { get; set; }

    [XmlElement("regionjname")]
    public required XrpcString RegionJpName { get; set; }

    [XmlElement("accuracy")]
    public required XrpcUByte Accuracy { get; set; }

    [XmlElement("latitude")]
    public required XrpcInt Latitude { get; set; }

    [XmlElement("longitude")]
    public required XrpcInt Longitude { get; set; }
}

public record FacilityLine
{
    [XmlElement("id")]
    public required XrpcString Id { get; set; }

    [XmlElement("class")]
    public required XrpcUByte Class { get; set; }

    [XmlElement("upclass")]
    public required XrpcUByte UpClass { get; set; }

    [XmlElement("rtt")]
    public required XrpcUShort Rtt { get; set; }
}

public record FacilityPortForward
{
    [XmlElement("globalip")]
    public required XrpcIp4 GlobalIp { get; set; }

    [XmlElement("globalport")]
    public required XrpcUShort GlobalPort { get; set; }

    [XmlElement("privateport")]
    public required XrpcUShort PrivatePort { get; set; }
}

public record FacilityPublic
{
    [XmlElement("flag")]
    public required XrpcUByte Flag { get; set; }

    [XmlElement("name")]
    public required XrpcString Name { get; set; }

    [XmlElement("latitude")]
    public required XrpcString Latitude { get; set; }

    [XmlElement("longitude")]
    public required XrpcString Longitude { get; set; }
}

public record FacilityShare
{
    [XmlElement("url")]
    public required FacilityUrl Url { get; set; }

    [XmlElement("eapass")]
    public required FacilityEaPass EaPass { get; set; }

    [XmlElement("eacoin")]
    public required FacilityEaCoin EaCoin { get; set; }
}

public record FacilityEaCoin
{
    [XmlElement("notchamount")]
    public required XrpcInt NotchAmount { get; set; }

    [XmlElement("notchcount")]
    public required XrpcInt NotchCount { get; set; }

    [XmlElement("supplylimit")]
    public required XrpcInt SupplyLimit { get; set; }
}

public record FacilityEaPass
{
    [XmlElement("valid")]
    public required XrpcUShort Valid { get; set; }
}

public record FacilityUrl
{
    [XmlElement("eapass")]
    public required XrpcString EaPass { get; set; }

    [XmlElement("arcadefan")]
    public required XrpcString ArcadeFan { get; set;}

    [XmlElement("konaminetdx")]
    public required XrpcString KonamiNetDx { get; set; }

    [XmlElement("konamiid")]
    public required XrpcString KonamiId { get; set; }

    [XmlElement("eagate")]
    public required XrpcString EaGate { get; set; }
}

public record FacilityCalendar
{
    [XmlElement("year")]
    public required XrpcShort Year { get; set; }

    // Real wire shape: <holiday __type="s16" __count="35">0 11 41 ...</holiday>
    // - a space-separated list of day-of-year offsets, not a repeated element.
    [XmlElement("holiday")]
    public required FacilityHolidayList Holiday { get; set; }
}

public class FacilityHolidayList
{
    [XmlAttribute("__type")]
    public string Type { get; set; } = "s16";

    [XmlAttribute("__count")]
    public int Count { get; set; }

    [XmlText]
    public string Value { get; set; } = "";
}
