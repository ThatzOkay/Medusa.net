using System.Xml.Serialization;

namespace Server.Models.Request;

[XmlRoot("eacoin")]
public class EaCoinCheckoutRequest
{
    [XmlElement("sessid")]
    public string sessionId { get; set; }
}