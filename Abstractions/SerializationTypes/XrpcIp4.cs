using System.Xml.Serialization;

namespace Abstractions.SerializationTypes;

public struct XrpcIp4(string value)
{
    [XmlAttribute("__type")]
    public string Type { get; set; } = "ip4";

    [XmlText]
    public string Value { get; set; } = value;

    public static implicit operator string(XrpcIp4 value)
    {
        return value.Value;
    }

    public static implicit operator XrpcIp4(string value)
    {
        return new XrpcIp4(value);
    }

    public bool Equals(XrpcIp4 other)
    {
        return Value == other.Value;
    }

    public override bool Equals(object obj)
    {
        if (obj is XrpcIp4 other)
        {
            return Equals(other);
        }
        return false;
    }

    public static bool operator ==(XrpcIp4 left, XrpcIp4 right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(XrpcIp4 left, XrpcIp4 right)
    {
        return !(left == right);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}