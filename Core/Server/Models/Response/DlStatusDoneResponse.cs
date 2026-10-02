using System.Xml.Serialization;

namespace Server.Models.Response;

[XmlRoot("dlstatus")]
[Serializable]
public class DlStatusDoneResponse
{
    [XmlAttribute("status")] public required int Status   { get; set; }
    [XmlElement("progress")] public required int Progress { get; set; }
}
