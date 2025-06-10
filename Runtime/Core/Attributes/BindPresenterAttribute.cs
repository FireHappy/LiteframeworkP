using System;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class BindPresenterAttribute : Attribute
{
    public Type PresenterType { get; }

    public BindPresenterAttribute(Type presenterType)
    {
        PresenterType = presenterType;
    }
}
