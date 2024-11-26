using System;

namespace Scrutor;

public class MissingTypeRegistrationException : InvalidOperationException
{
    public MissingTypeRegistrationException(Type serviceType)
        : base($"Could not find any registered services for type '{ReflectionExtensions.ToFriendlyName(serviceType)}'.")
    {
        ServiceType = serviceType;
    }

    public Type ServiceType { get; }
}