using System.Xml.Serialization;

namespace Server.Models.Request;

[XmlRoot("cardmng")]
public class AuthPassRequest
{
    [XmlAttribute("refid")]
    public required string ReferenceId { get; set; }
    
    [XmlAttribute("pass")]
    public required string  Password { get; set; }
}