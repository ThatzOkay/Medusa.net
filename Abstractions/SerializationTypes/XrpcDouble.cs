using System.Xml.Serialization;

namespace Abstractions.SerializationTypes;

public struct XrpcDouble
{
    [XmlAttribute("__type")]
    public string Type { get; set; }

    [XmlText]
    public double Value { get; set; }

    public XrpcDouble(double value)
    {
        Type = "double";
        Value = value;
    }

    public static implicit operator double(XrpcDouble value)
    {
        return value.Value;
    }

    public static implicit operator XrpcDouble(double value)
    {
        return new XrpcDouble(value);
    }

    public bool Equals(XrpcDouble other)
    {
        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        if (obj is XrpcDouble other)
        {
            return Equals(other);
        }
        return false;
    }

    public static bool operator ==(XrpcDouble left, XrpcDouble right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(XrpcDouble left, XrpcDouble right)
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
