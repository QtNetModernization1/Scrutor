using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;

namespace Scrutor;

internal sealed class LifetimeSelector : ILifetimeSelector, ISelector
{
    public LifetimeSelector(ServiceTypeSelector inner, IEnumerable<TypeMap> typeMaps, IEnumerable<TypeFactoryMap> typeFactoryMaps)
    {
        Inner = inner;
        TypeMaps = typeMaps;
        TypeFactoryMaps = typeFactoryMaps;
    }

    private ServiceTypeSelector Inner { get; }

    private IEnumerable<TypeMap> TypeMaps { get; }

    private IEnumerable<TypeFactoryMap> TypeFactoryMaps { get; }

    public Microsoft.Extensions.DependencyInjection.ServiceLifetime? Lifetime { get; set; }

    public IImplementationTypeSelector WithSingletonLifetime()
    {
        return WithLifetime(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton);
    }

    public IImplementationTypeSelector WithScopedLifetime()
    {
        return WithLifetime(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped);
    }

    public IImplementationTypeSelector WithTransientLifetime()
    {
        return WithLifetime(Microsoft.Extensions.DependencyInjection.ServiceLifetime.Transient);
    }

public IImplementationTypeSelector WithLifetime(Microsoft.Extensions.DependencyInjection.ServiceLifetime lifetime)
{
    var values = Enum.GetValues(typeof(Microsoft.Extensions.DependencyInjection.ServiceLifetime));
    if (!((IList)values).Contains(lifetime))
    {
        throw new ArgumentException($"The value of argument '{nameof(lifetime)}' ({lifetime}) is invalid for Enum type '{nameof(Microsoft.Extensions.DependencyInjection.ServiceLifetime)}'.", nameof(lifetime));
    }

    Inner.PropagateLifetime(lifetime);

    return this;
}

    #region Chain Methods

    public IImplementationTypeSelector FromCallingAssembly()
    {
        return Inner.FromCallingAssembly();
    }

    public IImplementationTypeSelector FromExecutingAssembly()
    {
        return Inner.FromExecutingAssembly();
    }

    public IImplementationTypeSelector FromEntryAssembly()
    {
        return Inner.FromEntryAssembly();
    }

    public IImplementationTypeSelector FromApplicationDependencies()
    {
        return Inner.FromApplicationDependencies();
    }

    public IImplementationTypeSelector FromApplicationDependencies(Func<Assembly, bool> predicate)
    {
        return Inner.FromApplicationDependencies(predicate);
    }

    public IImplementationTypeSelector FromAssemblyDependencies(Assembly assembly)
    {
        return Inner.FromAssemblyDependencies(assembly);
    }

    public IImplementationTypeSelector FromDependencyContext(DependencyContext context)
    {
        return Inner.FromDependencyContext(context);
    }

    public IImplementationTypeSelector FromDependencyContext(DependencyContext context, Func<Assembly, bool> predicate)
    {
        return Inner.FromDependencyContext(context, predicate);
    }

    public IImplementationTypeSelector FromAssemblyOf<T>()
    {
        return Inner.FromAssemblyOf<T>();
    }

    public IImplementationTypeSelector FromAssembliesOf(params Type[] types)
    {
        return Inner.FromAssembliesOf(types);
    }

    public IImplementationTypeSelector FromAssembliesOf(IEnumerable<Type> types)
    {
        return Inner.FromAssembliesOf(types);
    }

    public IImplementationTypeSelector FromAssemblies(params Assembly[] assemblies)
    {
        return Inner.FromAssemblies(assemblies);
    }

    public IImplementationTypeSelector FromAssemblies(IEnumerable<Assembly> assemblies)
    {
        return Inner.FromAssemblies(assemblies);
    }

    public IServiceTypeSelector AddClasses()
    {
        return Inner.AddClasses();
    }

    public IServiceTypeSelector AddClasses(bool publicOnly)
    {
        return Inner.AddClasses(publicOnly);
    }

    public IServiceTypeSelector AddClasses(Action<IImplementationTypeFilter> action)
    {
        return Inner.AddClasses(action);
    }

    public IServiceTypeSelector AddClasses(Action<IImplementationTypeFilter> action, bool publicOnly)
    {
        return Inner.AddClasses(action, publicOnly);
    }

    public ILifetimeSelector AsSelf()
    {
        return Inner.AsSelf();
    }

    public ILifetimeSelector As<T>()
    {
        return Inner.As<T>();
    }

    public ILifetimeSelector As(params Type[] types)
    {
        return Inner.As(types);
    }

    public ILifetimeSelector As(IEnumerable<Type> types)
    {
        return Inner.As(types);
    }

    public ILifetimeSelector AsImplementedInterfaces()
    {
        return Inner.AsImplementedInterfaces();
    }

    public ILifetimeSelector AsImplementedInterfaces(Func<Type, bool> predicate)
    {
        return Inner.AsImplementedInterfaces(predicate);
    }

    public ILifetimeSelector AsSelfWithInterfaces()
    {
        return Inner.AsSelfWithInterfaces();
    }

    public ILifetimeSelector AsMatchingInterface()
    {
        return Inner.AsMatchingInterface();
    }

    public ILifetimeSelector AsMatchingInterface(Action<Type, IImplementationTypeFilter> action)
    {
        return Inner.AsMatchingInterface(action);
    }

    public ILifetimeSelector As(Func<Type, IEnumerable<Type>> selector)
    {
        return Inner.As(selector);
    }

    public IImplementationTypeSelector UsingAttributes()
    {
        return Inner.UsingAttributes();
    }

    public IServiceTypeSelector UsingRegistrationStrategy(RegistrationStrategy registrationStrategy)
    {
        return Inner.UsingRegistrationStrategy(registrationStrategy);
    }

    #endregion

    void ISelector.Populate(IServiceCollection services, RegistrationStrategy? strategy)
    {
        strategy ??= RegistrationStrategy.Append;

        var lifetime = Lifetime ?? ServiceLifetime.Transient;

        foreach (var typeMap in TypeMaps)
        {
            foreach (var serviceType in typeMap.ServiceTypes)
            {
                var implementationType = typeMap.ImplementationType;

                if (!implementationType.IsBasedOn(serviceType))
                {
                    throw new InvalidOperationException($@"Type ""{implementationType.ToFriendlyName()}"" is not assignable to ""${serviceType.ToFriendlyName()}"".");
                }

                var descriptor = new ServiceDescriptor(serviceType, implementationType, lifetime);

                strategy.Apply(services, descriptor);
            }
        }

        foreach (var typeFactoryMap in TypeFactoryMaps)
        {
            foreach (var serviceType in typeFactoryMap.ServiceTypes)
            {
                var descriptor = new ServiceDescriptor(serviceType, typeFactoryMap.ImplementationFactory, lifetime);

                strategy.Apply(services, descriptor);
            }
        }
    }
}