using System.Xml.Serialization;
using Abstractions.SerializationTypes;

namespace Server.Models.Response;

[XmlRoot("eacoin")]
[Serializable]
public class EaCoinConsumeResponse
{
    [XmlAttribute("status")]
    public int Status { get; set; }

    [XmlElement("acstatus")]
    public XrpcUByte AcStatus { get; set; }
    
    [XmlElement("autocharge")]
    public XrpcUByte AutoCharge { get; set; }
    
    [XmlElement("balance")]
    public XrpcInt Balance { get; set; }
}