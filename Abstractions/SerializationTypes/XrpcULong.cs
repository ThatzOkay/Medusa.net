using System.Xml.Serialization;

namespace Abstractions.SerializationTypes;

public struct XrpcULong
{
    [XmlAttribute("__type")]
    public string Type { get; set; }

    [XmlText]
    public ulong Value { get; set; }

    public XrpcULong(byte value)
    {
        Value = value;
        Type = "u8";
    }

    public XrpcULong(ushort value)
    {
        Value = value;
        Type = "u16";
    }

    public XrpcULong(ulong value)
    {
        Value = value;
        Type = "u32";
    }
}