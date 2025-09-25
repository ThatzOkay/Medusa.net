using System.Xml.Serialization;

namespace Abstractions.SerializationTypes;

public struct XrpcULong(ulong value)
{
    [XmlAttribute("__type")]
    public string Type { get; set; } = "u64";

    [XmlText] public ulong Value { get; set; } = value;
}