using System.Xml.Serialization;

namespace Server.Models.Response;

[XmlRoot("eventlog")]
[Serializable]
public class WriteEventLogResponse
{
    [XmlAttribute("status")]
    public int Status { get; set; }
}
