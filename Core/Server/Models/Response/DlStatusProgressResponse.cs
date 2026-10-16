using System.Xml.Serialization;

namespace Server.Models.Response;

[XmlRoot("dlstatus")]
[Serializable]
public class DlStatusProgressResponse
{
    [XmlAttribute("status")] public required int Status { get; set; }
}
