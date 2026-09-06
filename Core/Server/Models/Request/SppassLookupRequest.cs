using System.Xml.Serialization;

namespace Server.Models.Request;

[XmlRoot("sppass")]
public class SppassLookupRequest
{
    [XmlAttribute("token")]
    public required string Token { get; set; }
}
