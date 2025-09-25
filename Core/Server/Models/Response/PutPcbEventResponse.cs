using System.Xml.Serialization;
using Abstractions.SerializationTypes;

namespace Server.Models.Response;

[XmlRoot("pcbevent")]
[Serializable]
public class PutPcbEventResponse
{
    [XmlAttribute("status")]
    public required int Status { get; set; }
}