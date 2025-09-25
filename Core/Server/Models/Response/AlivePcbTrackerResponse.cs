using System.Xml.Serialization;

namespace Server.Models.Response;

[XmlRoot("pcbtracker")]
[Serializable]
public class AlivePcbTrackerResponse
{
    [XmlAttribute("status")]
    public required  int Status { get; set; }
    
    [XmlAttribute("expire")]
    public required  int Expire { get; set; }
    
    [XmlAttribute("ecenable")]
    public required  int EcEnable { get; set; }
    
    [XmlAttribute("eclimit")]
    public required  int EcLimit { get; set; }
    
    [XmlAttribute("limit")]
    public required  int Limit { get; set; }
    
    [XmlAttribute("time")]
    public required long Time { get; set; }
}