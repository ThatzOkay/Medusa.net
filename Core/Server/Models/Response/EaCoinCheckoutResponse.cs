using System.Xml.Serialization;

namespace Server.Models.Response;

[XmlRoot("eacoin")]
public class EaCoinCheckoutResponse
{
    [XmlAttribute("status")]
    public int Status { get; set; }
}