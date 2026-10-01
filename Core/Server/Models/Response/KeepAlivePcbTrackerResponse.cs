using System.Xml.Serialization;

namespace Server.Models.Response;

[XmlRoot("pcbtracker")]
public class KeepAlivePcbTrackerResponse
{
    [XmlAttribute("status")]
    public required int Status { get; set; }
}
