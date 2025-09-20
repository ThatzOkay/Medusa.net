using System.Xml.Serialization;

namespace Abstractions.SerializationTypes;

public struct XrpcInt(int value)
{
    [XmlAttribute("__type")]
    public string Type { get; set; } = "s32";

    [XmlText]
    public int Value { get; set; } = value;
}