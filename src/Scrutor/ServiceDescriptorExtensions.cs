using System;
using Microsoft.Extensions.DependencyInjection;

namespace Scrutor;

internal static class ServiceDescriptorExtensions
{
    public static Microsoft.Extensions.DependencyInjection.ServiceDescriptor WithImplementationFactory(this Microsoft.Extensions.DependencyInjection.ServiceDescriptor descriptor, System.Func<System.IServiceProvider, object> implementationFactory) =>
        new Microsoft.Extensions.DependencyInjection.ServiceDescriptor(descriptor.ServiceType, implementationFactory, descriptor.Lifetime);

    public static Microsoft.Extensions.DependencyInjection.ServiceDescriptor WithServiceType(this Microsoft.Extensions.DependencyInjection.ServiceDescriptor descriptor, System.Type serviceType) => descriptor switch
    {
        { ImplementationType: not null } => new Microsoft.Extensions.DependencyInjection.ServiceDescriptor(serviceType, descriptor.ImplementationType, descriptor.Lifetime),
        { ImplementationFactory: not null } => new Microsoft.Extensions.DependencyInjection.ServiceDescriptor(serviceType, descriptor.ImplementationFactory, descriptor.Lifetime),
        { ImplementationInstance: not null } => new Microsoft.Extensions.DependencyInjection.ServiceDescriptor(serviceType, descriptor.ImplementationInstance),
        _ => throw new System.ArgumentException($"No implementation factory or instance or type found for {descriptor.ServiceType}.", nameof(descriptor))
    };
}