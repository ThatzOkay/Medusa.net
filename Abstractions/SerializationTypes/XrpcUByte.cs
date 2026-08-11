using System.Xml.Serialization;

namespace Abstractions.SerializationTypes;

public struct XrpcUByte
{
    [XmlAttribute("__type")]
    public string Type { get; set; }

    [XmlText]
    public byte Value { get; set; }

    public XrpcUByte(byte value)
    {
        Type = "u8";
        Value = value;
    }

    public static implicit operator byte(XrpcUByte value)
    {
        return value.Value;
    }

    public static implicit operator XrpcUByte(byte value)
    {
        return new XrpcUByte(value);
    }

    public bool Equals(XrpcUByte other)
    {
        return Value == other.Value;
    }

    public override bool Equals(object obj)
    {
        if (obj is XrpcUByte other)
        {
            return Equals(other);
        }

        return false;
    }

    public static bool operator ==(XrpcUByte left, XrpcUByte right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(XrpcUByte left, XrpcUByte right)
    {
        return !(left == right);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public static XrpcUByte FromBool(bool v)
    {
        return new XrpcUByte(v ? (byte)1 : (byte)0);
    }
}