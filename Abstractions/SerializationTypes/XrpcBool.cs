using System.Xml.Serialization;

namespace Abstractions.SerializationTypes;

public struct XrpcBool
{
    [XmlAttribute("__type")]
    public string Type { get; set; }

    [XmlText]
    public int Value { get; set; }

    public XrpcBool(bool value)
    {
        Type = "bool";
        Value = (value ? 1 : 0);
    }

    public XrpcBool(int value)
        : this(value != 0)
    {
    }

    public static implicit operator bool(XrpcBool value)
    {
        return value.Value != 0;
    }

    public static implicit operator XrpcBool(bool value)
    {
        return new XrpcBool(value);
    }

    public bool Equals(XrpcBool other)
    {
        return Value == other.Value;
    }

    public override bool Equals(object obj)
    {
        if (obj is XrpcBool other)
        {
            return Equals(other);
        }
        return false;
    }

    public static bool operator ==(XrpcBool left, XrpcBool right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(XrpcBool left, XrpcBool right)
    {
        return !(left == right);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}