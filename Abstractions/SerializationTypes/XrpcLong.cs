using System.Xml.Serialization;

namespace Abstractions.SerializationTypes;

public struct XrpcLong(long value)
{
    [XmlAttribute("__type")]
    public string Type { get; set; } = "s64";
    
    [XmlText]
    public long Value { get; set; } = value;
}