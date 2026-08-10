using System.Xml.Serialization;

namespace Abstractions.SerializationTypes;

public struct XrpcString
{
    [XmlAttribute("__type")]
    public string Type { get; set; }

    [XmlText]
    public string Value { get; set; }

    public XrpcString(string value)
    {
        Type = "str";
        Value = value;
    }

    public static implicit operator string(XrpcString value)
    {
        return value.Value;
    }

    public static implicit operator XrpcString(string value)
    {
        return new XrpcString(value);
    }

    public bool Equals(XrpcString other)
    {
        return Value == other.Value;
    }

    public override bool Equals(object obj)
    {
        if (obj is XrpcString other)
        {
            return Equals(other);
        }
        return false;
    }

    public static bool operator ==(XrpcString left, XrpcString right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(XrpcString left, XrpcString right)
    {
        return !(left == right);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}
