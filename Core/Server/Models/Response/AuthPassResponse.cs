using System.Xml.Serialization;

namespace Server.Models.Response;

[XmlRoot("cardmng")]
[Serializable]
public class AuthPassResponse
{
    [XmlAttribute("status")]
    public required int Status { get; set; }
}