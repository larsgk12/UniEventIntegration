namespace UniEventIntegration.Models;

[AttributeUsage(AttributeTargets.Class)]
public sealed class BizEntityAttribute(string? route = null) : Attribute
{
    public string? Route => route;
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class BizCustomFieldAttribute : Attribute { }
