using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Scrutor;

internal static class ServiceDescriptorExtensions
{
    public static ServiceDescriptor WithImplementationFactory(this ServiceDescriptor descriptor, Func<IServiceProvider, object> implementationFactory) =>
        new(descriptor.ServiceType, implementationFactory, descriptor.Lifetime);

#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP3_1_OR_GREATER || NET6_0_OR_GREATER
    public static ServiceDescriptor WithServiceType(this ServiceDescriptor descriptor, Type serviceType) => descriptor switch
    {
        { ImplementationType: not null } => new ServiceDescriptor(serviceType, descriptor.ImplementationType, descriptor.Lifetime),
        { ImplementationFactory: not null } => new ServiceDescriptor(serviceType, descriptor.ImplementationFactory, descriptor.Lifetime),
        { ImplementationInstance: not null } => new ServiceDescriptor(serviceType, descriptor.ImplementationInstance),
        _ => throw new ArgumentException($"No implementation factory or instance or type found for {descriptor.ServiceType}.", nameof(descriptor))
    };
#else
public static ServiceDescriptor WithServiceType(this ServiceDescriptor descriptor, System.Type serviceType)
{
    if (descriptor.ImplementationType != null)
    {
        return new ServiceDescriptor(serviceType, descriptor.ImplementationType, descriptor.Lifetime);
    }
    if (descriptor.ImplementationFactory != null)
    {
        return new ServiceDescriptor(serviceType, descriptor.ImplementationFactory, descriptor.Lifetime);
    }
    if (descriptor.ImplementationInstance != null)
    {
        return new ServiceDescriptor(serviceType, descriptor.ImplementationInstance);
    }
    throw new ArgumentException($"No implementation factory or instance or type found for {descriptor.ServiceType}.", nameof(descriptor));
}
#endif
}