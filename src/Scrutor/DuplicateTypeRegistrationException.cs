using System;

namespace Scrutor;

public class DuplicateTypeRegistrationException : InvalidOperationException
{
    public DuplicateTypeRegistrationException(Type serviceType)
        : base($"A service of type '{TypeExtensions.ToFriendlyName(serviceType)}' has already been registered.")
    {
        ServiceType = serviceType;
    }

    public Type ServiceType { get; }
}