using System.Xml.Serialization;

namespace Abstractions.SerializationTypes;

public struct XrpcInt
{
    [XmlAttribute("__type")]
    public string Type { get; set; }

    [XmlText]
    public int Value { get; set; }

    public XrpcInt(int value)
    {
        Type = "s32";
        Value = value;
    }

    public static implicit operator int(XrpcInt value)
    {
        return value.Value;
    }

    public static implicit operator XrpcInt(int value)
    {
        return new XrpcInt(value);
    }

    public bool Equals(XrpcInt other)
    {
        return Value == other.Value;
    }

    public override bool Equals(object obj)
    {
        if (obj is XrpcInt other)
        {
            return Equals(other);
        }
        return false;
    }

    public static bool operator ==(XrpcInt left, XrpcInt right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(XrpcInt left, XrpcInt right)
    {
        return !(left == right);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public static XrpcInt FromBool(bool v)
    {
        return new XrpcInt(v ? 1 : 0);
    }
}
