using System.Xml.Serialization;

namespace Server.Models.Response;

[XmlRoot("package")]
[Serializable]
public class ListPackageResponse
{
    [XmlAttribute("status")]
    public required  int Status { get; set; }
    
    [XmlAttribute("expire")]
    public required  int Expire { get; set; }
}