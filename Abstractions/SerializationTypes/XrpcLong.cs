using System;
using System.Xml.Serialization;

namespace Abstractions.SerializationTypes;

public struct XrpcLong : IEquatable<XrpcLong>
{
    [XmlAttribute("__type")]
    public string Type { get; set; }

    [XmlText]
    public long Value { get; set; }

    public XrpcLong(long value)
    {
        Type = "s64";
        Value = value;
    }

    public static implicit operator long(XrpcLong value)
    {
        return value.Value;
    }

    public static implicit operator XrpcLong(long value)
    {
        return new XrpcLong(value);
    }

    public bool Equals(XrpcLong other)
    {
        return Value == other.Value;
    }

    public override bool Equals(object obj)
    {
        if (obj is XrpcLong other)
        {
            return Equals(other);
        }
        return false;
    }

    public static bool operator ==(XrpcLong left, XrpcLong right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(XrpcLong left, XrpcLong right)
    {
        return !(left == right);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}
