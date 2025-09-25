using System.Xml.Serialization;

namespace Server.Models.Response;

[XmlRoot("cardmng")]
[Serializable]
public class GetRefIdResponse
{
    [XmlAttribute("status")]
    public int Status { get; set; }
    
    [XmlAttribute("refid")]
    public required string ReferenceId { get; set; }
    
    [XmlAttribute("dataid")]
    public required string DataId { get; set; }
}