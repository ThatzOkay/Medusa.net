using System.Xml.Serialization;

namespace Abstractions.SerializationTypes;

public struct XrpcLong
{
    [XmlAttribute("__type")]
    public string Type { get; set; }

    [XmlText]
    public long Value { get; set; }

    public XrpcLong(byte value)
    {
        Value = value;
        Type = "s8";
    }

    public XrpcLong(short value)
    {
        Value = value;
        Type = "s16";
    }

    public XrpcLong(long value)
    {
        Value = value;
        Type = "s32";
    }
}