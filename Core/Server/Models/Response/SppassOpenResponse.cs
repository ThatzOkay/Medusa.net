using System.Xml.Serialization;

namespace Server.Models.Response;

[XmlRoot("sppass")]
public class SppassOpenResponse
{
    [XmlAttribute("status")]
    public required int Status { get; set; }

    [XmlAttribute("token")]
    public required string Token { get; set; }

    [XmlAttribute("expire_datetime")]
    public required string ExpireDatetime { get; set; }

    [XmlAttribute("url")]
    public required string Url { get; set; }

    [XmlAttribute("interval")]
    public required int Interval { get; set; }
}
