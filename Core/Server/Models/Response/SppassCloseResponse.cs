using System.Xml.Serialization;

namespace Server.Models.Response;

[XmlRoot("sppass")]
public class SppassCloseResponse
{
    [XmlAttribute("status")]
    public required int Status { get; set; }
}
