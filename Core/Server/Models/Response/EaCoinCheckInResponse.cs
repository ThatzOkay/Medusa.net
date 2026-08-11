using System.Xml.Serialization;
using Abstractions.SerializationTypes;

namespace Server.Models.Response;

[XmlRoot("eacoin")]
[Serializable]
public class EaCoinCheckInResponse
{
    [XmlAttribute("status")]
    public required  int Status { get; set; }
    
    [XmlAttribute("expire")]
    public required  int Expire { get; set; }
    
    [XmlElement("sequence")]
    public XrpcShort Sequence { get; set; }
    
    [XmlElement("acstatus")]
    public XrpcUByte AcStatus { get; set; }
    
    [XmlElement("acid")]
    public XrpcString AcId { get; set; }
    
    [XmlElement("acname")]
    public XrpcString AcName { get; set; }
    
    [XmlElement("balance")]
    public XrpcInt Balance { get; set; }

    [XmlElement("sessid")]
    public XrpcString SessionId { get; set; }

    [XmlElement("inshopcharge")]
    public XrpcUByte InShopCharge { get; set; }

    [XmlElement("point")]
    public XrpcInt Point { get; set; }
}