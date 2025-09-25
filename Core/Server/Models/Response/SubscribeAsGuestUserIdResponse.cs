using System.Xml.Serialization;

namespace Server.Models.Response;

[XmlRoot("userid")]
[Serializable]
public class SubscribeAsGuestUserIdResponse
{
    [XmlAttribute("status")]
    public int Status { get; set; }
}