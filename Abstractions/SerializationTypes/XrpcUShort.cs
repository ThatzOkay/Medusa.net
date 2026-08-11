using System.Xml.Serialization;

namespace Abstractions.SerializationTypes;

public struct XrpcUShort
{
    [XmlAttribute("__type")]
    public string Type { get; set; }

    [XmlText]
    public ushort Value { get; set; }

    public XrpcUShort(ushort value)
    {
        Type = "u16";
        Value = value;
    }

    public static implicit operator ushort(XrpcUShort value)
    {
        return value.Value;
    }

    public static implicit operator XrpcUShort(ushort value)
    {
        return new XrpcUShort(value);
    }

    public bool Equals(XrpcUShort other)
    {
        return Value == other.Value;
    }

    public override bool Equals(object obj)
    {
        if (obj is XrpcUShort other)
        {
            return Equals(other);
        }
        return false;
    }

    public static bool operator ==(XrpcUShort left, XrpcUShort right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(XrpcUShort left, XrpcUShort right)
    {
        return !(left == right);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}
