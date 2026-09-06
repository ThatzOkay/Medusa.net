using System.Xml.Serialization;

namespace Server.Models.Response;

[XmlRoot("sppass")]
public class SppassLookupResponse
{
    [XmlAttribute("status")]
    public required int Status { get; set; }

    [XmlAttribute("url")]
    public required string Url { get; set; }

    [XmlAttribute("interval")]
    public required int Interval { get; set; }

    [XmlAttribute("card_type")]
    public string CardType { get; set; } = "";

    [XmlAttribute("card_id")]
    public string CardId { get; set; } = "";
}
