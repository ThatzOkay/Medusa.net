using System.Xml.Serialization;

namespace Abstractions.SerializationTypes;

public struct XrpcIp4(string value)
{
    [XmlAttribute("__type")]
    public string Type { get; set; } = "ip4";

    [XmlText]
    public string Value { get; set; } = value;
}