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
    
    [XmlElement("portforward")]
    public required FacilityPortForward PortForward { get; set; }
    
    [XmlElement("public")]
    public required FacilityPublic Public { get; set; }
    
    [XmlElement("share")]
    public required FacilityShare Share { get; set; }
}

public record FacilityLocation
{
    [XmlElement("id")]
    public required XrpcString Id { get; set; }
    
    [XmlElement("country")]
    public required XrpcString Country { get; set; }
    
    [XmlElement("region")]
    public required XrpcString Region { get; set; }

    [XmlElement("customercode")]
    public required XrpcString CustomerCode { get; set; }

    [XmlElement("companycode")]
    public required XrpcString CompanyCode { get; set; }

    [XmlElement("latitude")]
    public required XrpcLong Latitude { get; set; }

    [XmlElement("longitude")]
    public required XrpcLong Longitude { get; set; }

    [XmlElement("accuracy")]
    public required XrpcULong Accuracy { get; set; }

    [XmlElement("countryname")]
    public required XrpcString CountryName { get; set; }

    [XmlElement("regionname")]
    public required XrpcString RegionName { get; set; }

    [XmlElement("countryjname")]
    public required XrpcString CountryJName { get; set; }

    [XmlElement("regionjname")]
    public required XrpcString RegionJName { get; set; }


    [XmlElement("name")]
    public required XrpcString Name { get; set; }
    
    [XmlElement("type")]
    public required XrpcULong Type { get; set; }
}

public record FacilityLine
{
    [XmlElement("class")]
    public required XrpcULong Class { get; set; }

    [XmlElement("rtt")]
    public required XrpcULong RTT { get; set; }

    [XmlElement("upclass")]
    public required XrpcULong UpClass { get; set; }

    [XmlElement("id")]
    public required XrpcString Id { get; set; }
}

public record FacilityPortForward
{
    [XmlElement("globalip")]
    public required XrpcIp4 GlobalIp { get; set; }
    
    [XmlElement("globalport")]
    public required XrpcULong GlobalPort { get; set; }
    
    [XmlElement("privateport")]
    public required XrpcULong PrivatePort { get; set; }
}

public record FacilityPublic
{
    [XmlElement("flag")]
    public required XrpcULong Flag { get; set; }
    
    [XmlElement("name")]
    public required XrpcString Name { get; set; }
    
    [XmlElement("latitude")]
    public required XrpcString Latitude { get; set; }
    
    [XmlElement("longitude")]
    public required XrpcString Longitude { get; set; }
}

public record FacilityShare
{
    [XmlElement("eacon")]
    public required FacilityEaCoin EaCoin { get; set; }

    [XmlElement("eapass")]
    public required FacilityEaPass EaPass { get; set; }
    
    [XmlElement("url")]
    public required FacilityUrl Url { get; set; }
}

public record FacilityEaCoin
{
    [XmlElement("notchamount")]
    public required XrpcLong NotchAmount { get; set; }
    
    [XmlElement("notchcount")]
    public required XrpcLong NotchCount { get; set; }
    
    [XmlElement("supplylimit")]
    public required XrpcLong SupplyLimit { get; set; }
    
}

public record FacilityEaPass
{
    public required XrpcULong Valid { get; set; }
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