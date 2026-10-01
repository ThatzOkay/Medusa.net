using System.Xml.Serialization;
using Abstractions.SerializationTypes;

namespace Server.Models.Response;

[XmlRoot("sppass")]
public class SppassLookupResponse
{
    [XmlAttribute("status")]
    public required int Status { get; set; }

    [XmlElement("url")]
    public required XrpcString Url { get; set; }

    [XmlElement("interval")]
    public required XrpcInt Interval { get; set; }

    [XmlElement("card_type")]
    public XrpcString CardType { get; set; } = "";

    [XmlElement("card_id")]
    public XrpcString CardId { get; set; } = "";
}
