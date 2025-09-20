using System.Xml.Serialization;

namespace Abstractions.SerializationTypes;

public struct XrpcString(string value)
{
    [XmlAttribute("__type")]
    public string Type { get; set; } = "str";

    [XmlText]
    public string Value { get; set; } = value;
}