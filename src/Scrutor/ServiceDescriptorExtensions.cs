using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Scrutor;

internal static class ServiceDescriptorExtensions
{
    public static ServiceDescriptor WithImplementationFactory(this ServiceDescriptor descriptor, Func<IServiceProvider, object> implementationFactory) =>
        new(descriptor.ServiceType, implementationFactory, descriptor.Lifetime);

    public static ServiceDescriptor WithServiceType(this ServiceDescriptor descriptor, Type serviceType) => descriptor switch
    {
        { ImplementationType: Type implementationType } when implementationType != null =>
            new ServiceDescriptor(serviceType, implementationType, descriptor.Lifetime),
        { ImplementationFactory: Func<IServiceProvider, object> factory } when factory != null =>
            new ServiceDescriptor(serviceType, sp => factory(sp), descriptor.Lifetime),
        { ImplementationInstance: object instance } when instance != null =>
            new ServiceDescriptor(serviceType, instance),
        _ => throw new ArgumentException($"No implementation factory or instance or type found for {descriptor.ServiceType}.", nameof(descriptor))
    };
}