using System.Xml.Serialization;

namespace Server.Models.Request;

[XmlRoot("cardmng")]
public class BindModelRequest
{
    [XmlAttribute("newflag")]
    public bool NewFlag { get; set; }
    
    [XmlAttribute("refid")]
    public required string ReferenceId { get; set; }
}