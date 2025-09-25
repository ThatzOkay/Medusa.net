using System.Xml.Serialization;

namespace Server.Models.Request;

[XmlRoot("pcbevent")]
public class PutPcbEventRequest
{
    [XmlElement("time")]
    public required string Time { get; set; }
}