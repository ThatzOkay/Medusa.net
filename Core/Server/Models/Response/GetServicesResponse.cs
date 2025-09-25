using System.Xml.Serialization;

namespace Server.Models.Response;

[XmlRoot("services")]
[Serializable]
public class GetServicesResponse
{
    [XmlAttribute("expire")]
    public int Expire { get; set; }

    [XmlAttribute("method")]
    public required  string Method { get; set; }
    
    [XmlAttribute("mode")]
    public required  string Mode { get; set; }
    
    [XmlAttribute("status")]
    public int Status { get; set; }
    
    [XmlElement("item")] 
    public List<ServiceItem> Items { get; set; } = [];
}

public record ServiceItem
{
    [XmlAttribute("name")]
    public required  string Name { get; set; }

    [XmlAttribute("url")]
    public required  string Url { get; set; }
}