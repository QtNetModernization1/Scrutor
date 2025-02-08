using System;
using Microsoft.Extensions.DependencyInjection;
#if NET8_0
using System.Runtime.InteropServices;
#endif

namespace Scrutor;

public abstract class DecorationStrategy
{
    protected DecorationStrategy(Type serviceType)
    {
        ServiceType = serviceType;
    }
    
    public Type ServiceType { get; }
    
    public abstract bool CanDecorate(Type serviceType);
    
    public abstract Func<IServiceProvider, object> CreateDecorator(Type serviceType);
    
    internal static DecorationStrategy WithType(Type serviceType, Type decoratorType) => 
        Create(serviceType, decoratorType, decoratorFactory: null);

    internal static DecorationStrategy WithFactory(Type serviceType, Func<object, IServiceProvider, object> decoratorFactory) => 
        Create(serviceType, decoratorType: null, decoratorFactory);
    
    protected static Func<IServiceProvider, object> TypeDecorator(Type serviceType, Type decoratorType) => serviceProvider =>
    {
        var instanceToDecorate = serviceProvider.GetRequiredService(serviceType);
#if NET8_0
        return RuntimeHelpers.GetUninitializedObject(decoratorType);
#else
        return ActivatorUtilities.CreateInstance(serviceProvider, decoratorType, instanceToDecorate);
#endif
    };

    protected static Func<IServiceProvider, object> FactoryDecorator(Type decorated, Func<object, IServiceProvider, object> decoratorFactory) => serviceProvider =>
    {
        var instanceToDecorate = serviceProvider.GetRequiredService(decorated);
        return decoratorFactory(instanceToDecorate, serviceProvider);
    };

    private static DecorationStrategy Create(Type serviceType, Type? decoratorType, Func<object, IServiceProvider, object>? decoratorFactory)
    {
        if (serviceType.IsOpenGeneric())
        {
            return new OpenGenericDecorationStrategy(serviceType, decoratorType, decoratorFactory);
        }

        return new ClosedTypeDecorationStrategy(serviceType, decoratorType, decoratorFactory);
    }
}