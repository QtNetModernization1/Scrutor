using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Scrutor;

public abstract class DecorationStrategy
{
    protected DecorationStrategy(System.Type serviceType)
    {
        ServiceType = serviceType;
    }

    protected static T GetRequiredService<T>(System.IServiceProvider serviceProvider) => (T)serviceProvider.GetService(typeof(T)) ?? throw new System.InvalidOperationException($"Failed to resolve service of type {typeof(T)}");

    public System.Type ServiceType { get; }

    public abstract bool CanDecorate(System.Type serviceType);

    public abstract System.Func<System.IServiceProvider, System.Object> CreateDecorator(System.Type serviceType);

    internal static DecorationStrategy WithType(System.Type serviceType, System.Type decoratorType) =>
        Create(serviceType, decoratorType, decoratorFactory: null);

    internal static DecorationStrategy WithFactory(System.Type serviceType, System.Func<System.Object, System.IServiceProvider, System.Object> decoratorFactory) =>
        Create(serviceType, decoratorType: null, decoratorFactory);

    protected static System.Func<System.IServiceProvider, System.Object> TypeDecorator(System.Type serviceType, System.Type decoratorType) => serviceProvider =>
    {
        var instanceToDecorate = serviceProvider.GetRequiredService(serviceType);
        return ActivatorUtilities.CreateInstance(serviceProvider, decoratorType, instanceToDecorate);
    };

    protected static System.Func<System.IServiceProvider, System.Object> FactoryDecorator(System.Type decorated, System.Func<System.Object, System.IServiceProvider, System.Object> decoratorFactory) => serviceProvider =>
    {
        var instanceToDecorate = GetRequiredService<System.Object>(serviceProvider);
        return decoratorFactory(instanceToDecorate, serviceProvider);
    };

    private static DecorationStrategy Create(System.Type serviceType, System.Type? decoratorType, System.Func<System.Object, System.IServiceProvider, System.Object>? decoratorFactory)
    {
        if (serviceType.IsOpenGeneric())
        {
            return new OpenGenericDecorationStrategy(serviceType, decoratorType, decoratorFactory);
        }

        return new ClosedTypeDecorationStrategy(serviceType, decoratorType, decoratorFactory);
    }
}