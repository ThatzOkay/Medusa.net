using System.Xml.Serialization;

namespace Server.Models.Request;

[XmlRoot("sppass")]
public class SppassCloseRequest
{
    [XmlAttribute("token")]
    public required string Token { get; set; }
}
