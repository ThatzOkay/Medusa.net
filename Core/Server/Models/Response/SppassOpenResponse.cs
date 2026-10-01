using System.Xml.Serialization;
using Abstractions.SerializationTypes;

namespace Server.Models.Response;

[XmlRoot("sppass")]
public class SppassOpenResponse
{
    [XmlAttribute("status")]
    public required int Status { get; set; }

    [XmlElement("token")]
    public required XrpcString Token { get; set; }

    [XmlElement("expire_datetime")]
    public required XrpcString ExpireDatetime { get; set; }

    [XmlElement("url")]
    public required XrpcString Url { get; set; }

    [XmlElement("interval")]
    public required XrpcInt Interval { get; set; }
}
