namespace Server.Attributes;


[AttributeUsage(AttributeTargets.Class)]
public class HandlerAttribute(string module, string method) : Attribute
{
    public readonly string Module = module;
    public readonly string Method = method;
}
