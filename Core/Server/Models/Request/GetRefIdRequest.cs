using System.Xml.Serialization;

namespace Server.Models.Request;

[XmlRoot("cardmng")]
public class GetRefIdRequest
{
    [XmlAttribute("cardid")]
    public required string CardId { get; set; }
    
    [XmlAttribute("passwd")]
    public required string  Password { get; set; }
}