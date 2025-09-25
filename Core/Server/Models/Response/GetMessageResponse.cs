using System.Xml.Serialization;

namespace Server.Models.Response;

[XmlRoot("message")]
[Serializable]
public class GetMessageResponse
{
    [XmlAttribute("status")]
    public required  int Status { get; set; }
    
    [XmlAttribute("expire")]
    public required  int Expire { get; set; }
}