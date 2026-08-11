using System.Xml.Serialization;

namespace Server.Models.Request;

[XmlRoot("eacoin")]
public class EaCoinCheckInRequest
{
    [XmlElement("slotid")]
    public required string SlotId { get; set; }
    [XmlElement("cardtype")]
    public required string CardType { get; set; }
    [XmlElement("cardid")]
    public required string CardId { get; set; }
    [XmlElement("passwd")]
    public required string PassWd { get; set; }
    [XmlElement("ectype")]
    public required string EcType { get; set; }
}