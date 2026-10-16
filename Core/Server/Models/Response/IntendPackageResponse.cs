using System.Xml.Serialization;

namespace Server.Models.Response;

[XmlRoot("package")]
[Serializable]
public class IntendPackageResponse
{
    [XmlAttribute("status")]
    public required int Status { get; set; }
}
