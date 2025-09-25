using System.Xml.Serialization;

namespace Server.Models.Request;

[XmlRoot("cardmng")]
public class InquireRequest
{
    [XmlAttribute("cardid")]
    public required string CardId { get; set; }
}