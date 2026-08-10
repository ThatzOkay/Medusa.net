using System.Xml.Serialization;

namespace Abstractions.SerializationTypes;

public struct XrpcShort
{
    [XmlAttribute("__type")]
    public string Type { get; set; }

    [XmlText]
    public short Value { get; set; }

    public XrpcShort(short value)
    {
        Type = "s16";
        Value = value;
    }

    public static implicit operator short(XrpcShort value)
    {
        return value.Value;
    }

    public static implicit operator XrpcShort(short value)
    {
        return new XrpcShort(value);
    }

    public bool Equals(XrpcShort other)
    {
        return Value == other.Value;
    }

    public override bool Equals(object obj)
    {
        if (obj is XrpcShort other)
        {
            return Equals(other);
        }
        return false;
    }

    public static bool operator ==(XrpcShort left, XrpcShort right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(XrpcShort left, XrpcShort right)
    {
        return !(left == right);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}
