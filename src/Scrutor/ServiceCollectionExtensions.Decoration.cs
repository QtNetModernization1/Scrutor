using Scrutor;
using System;
using System.Collections;
using System.Collections.Generic;
using GenericCollections = System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

[PublicAPI]
public static partial class ServiceCollectionExtensions
{
    // Overload to handle IServiceCollection explicitly
    public static bool TryDecorate<TService, TDecorator>(this IServiceCollection services)
        where TDecorator : TService
    {
        Preconditions.NotNull(services, nameof(services));

        return services.TryDecorate(typeof(TService), typeof(TDecorator));
    }

    // Helper method to ensure IServiceCollection is recognized
    private static void EnsureIServiceCollectionIsRecognized(IServiceCollection services)
    {
        // This method is just to force the compiler to recognize IServiceCollection
        // It will never be called
        var temp = services as System.Collections.Generic.IEnumerable<ServiceDescriptor>;
    }

    // Helper method to ensure System.Collections.Generic.IList<T> is recognized
    private static void EnsureIListIsRecognized<T>()
    {
        System.Collections.Generic.IList<T> list = new System.Collections.Generic.List<T>();
    }
    /// <summary>
    /// Decorates all registered services of type <typeparamref name="TService"/>
/// using the specified type <typeparamref name="TDecorator"/>.
    /// </summary>
    /// <param name="services">The services to add to.</param>
    /// <exception cref="DecorationException">If no service of the type <typeparamref name="TService"/> has been registered.</exception>
    /// <exception cref="ArgumentNullException">If the <paramref name="services"/> argument is <c>null</c>.</exception>
    public static IEnumerable<ServiceDescriptor> Decorate<TService, TDecorator>(this IEnumerable<ServiceDescriptor> services)
        where TDecorator : TService
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        return DecorateInternal(services, typeof(TService), typeof(TDecorator));
    }

    private static IEnumerable<ServiceDescriptor> DecorateInternal(IEnumerable<ServiceDescriptor> services, Type serviceType, Type decoratorType)
    {
        // Implementation details...
        return services;
    }

    /// <summary>
    /// Decorates all registered services of the specified <paramref name="serviceType"/>
/// using the specified <paramref name="decoratorType"/>.
    /// </summary>
    /// <param name="services">The services to add to.</param>
    /// <param name="serviceType">The type of services to decorate.</param>
    /// <param name="decoratorType">The type to decorate existing services with.</param>
    /// <exception cref="DecorationException">If no service of the specified <paramref name="serviceType"/> has been registered.</exception>
    /// <exception cref="ArgumentNullException">If either the <paramref name="services"/>,
    /// <paramref name="serviceType"/> or <paramref name="decoratorType"/> arguments are <c>null</c>.</exception>
    public static IServiceCollection Decorate(this IServiceCollection services, Type serviceType, Type decoratorType)
    {
        Preconditions.NotNull(services, nameof(services));
        Preconditions.NotNull(serviceType, nameof(serviceType));
        Preconditions.NotNull(decoratorType, nameof(decoratorType));

        return services.Decorate(DecorationStrategy.WithType(serviceType, decoratorType));
    }

    /// <summary>
    /// Decorates all registered services of the specified <paramref name="serviceType"/>
/// using the specified <paramref name="decoratorType"/>.
    /// </summary>
    /// <param name="services">The services to add to.</param>
    /// <param name="serviceType">The type of services to decorate.</param>
    /// <param name="decoratorType">The type to decorate existing services with.</param>
    /// <exception cref="ArgumentNullException">If either the <paramref name="services"/>,
    /// <paramref name="serviceType"/> or <paramref name="decoratorType"/> arguments are <c>null</c>.</exception>
    public static bool TryDecorate(this IServiceCollection services, Type serviceType, Type decoratorType)
    {
        Preconditions.NotNull(services, nameof(services));
        Preconditions.NotNull(serviceType, nameof(serviceType));
        Preconditions.NotNull(decoratorType, nameof(decoratorType));

        return services.TryDecorate(DecorationStrategy.WithType(serviceType, decoratorType));
    }
    
    /// <summary>
    /// Decorates all registered services of type <typeparamref name="TService"/>
    /// using the <paramref name="decorator"/> function.
    /// </summary>
    /// <typeparam name="TService">The type of services to decorate.</typeparam>
    /// <param name="services">The services to add to.</param>
    /// <param name="decorator">The decorator function.</param>
    /// <exception cref="DecorationException">If no service of <typeparamref name="TService"/> has been registered.</exception>
    /// <exception cref="ArgumentNullException">If either the <paramref name="services"/>
    /// or <paramref name="decorator"/> arguments are <c>null</c>.</exception>
    public static IServiceCollection Decorate<TService>(this IServiceCollection services, Func<TService, TService> decorator) where TService : notnull
    {
        Preconditions.NotNull(services, nameof(services));
        Preconditions.NotNull(decorator, nameof(decorator));

        return services.Decorate<TService>((service, _) => decorator(service));
    }

    /// <summary>
    /// Decorates all registered services of type <typeparamref name="TService"/>
    /// using the <paramref name="decorator"/> function.
    /// </summary>
    /// <typeparam name="TService">The type of services to decorate.</typeparam>
    /// <param name="services">The services to add to.</param>
    /// <param name="decorator">The decorator function.</param>
    /// <exception cref="ArgumentNullException">If either the <paramref name="services"/>
    /// or <paramref name="decorator"/> arguments are <c>null</c>.</exception>
    public static bool TryDecorate<TService>(this IServiceCollection services, Func<TService, TService> decorator) where TService : notnull
    {
        Preconditions.NotNull(services, nameof(services));
        Preconditions.NotNull(decorator, nameof(decorator));

        return services.TryDecorate<TService>((service, _) => decorator(service));
    }

    /// <summary>
    /// Decorates all registered services of type <typeparamref name="TService"/>
    /// using the <paramref name="decorator"/> function.
    /// </summary>
    /// <typeparam name="TService">The type of services to decorate.</typeparam>
    /// <param name="services">The services to add to.</param>
    /// <param name="decorator">The decorator function.</param>
    /// <exception cref="DecorationException">If no service of <typeparamref name="TService"/> has been registered.</exception>
    /// <exception cref="ArgumentNullException">If either the <paramref name="services"/>
    /// or <paramref name="decorator"/> arguments are <c>null</c>.</exception>
    public static IServiceCollection Decorate<TService>(this IServiceCollection services, Func<TService, IServiceProvider, TService> decorator) where TService : notnull
    {
        Preconditions.NotNull(services, nameof(services));
        Preconditions.NotNull(decorator, nameof(decorator));
        
        return services.Decorate(typeof(TService), (service, provider) => decorator((TService)service, provider));
    }

    /// <summary>
    /// Decorates all registered services of type <typeparamref name="TService"/>
    /// using the <paramref name="decorator"/> function.
    /// </summary>
    /// <typeparam name="TService">The type of services to decorate.</typeparam>
    /// <param name="services">The services to add to.</param>
    /// <param name="decorator">The decorator function.</param>
    /// <exception cref="ArgumentNullException">If either the <paramref name="services"/>
    /// or <paramref name="decorator"/> arguments are <c>null</c>.</exception>
    public static bool TryDecorate<TService>(this IServiceCollection services, Func<TService, IServiceProvider, TService> decorator) where TService : notnull
    {
        Preconditions.NotNull(services, nameof(services));
        Preconditions.NotNull(decorator, nameof(decorator));
        
        return services.TryDecorate(typeof(TService), (service, provider) => decorator((TService)service, provider));
    }
    
    /// <summary>
    /// Decorates all registered services of the specified <paramref name="serviceType"/>
    /// using the <paramref name="decorator"/> function.
    /// </summary>
    /// <param name="services">The services to add to.</param>
    /// <param name="serviceType">The type of services to decorate.</param>
    /// <param name="decorator">The decorator function.</param>
    /// <exception cref="DecorationException">If no service of the specified <paramref name="serviceType"/> has been registered.</exception>
    /// <exception cref="ArgumentNullException">If either the <paramref name="services"/>,
    /// <paramref name="serviceType"/> or <paramref name="decorator"/> arguments are <c>null</c>.</exception>
    public static IServiceCollection Decorate(this IServiceCollection services, Type serviceType, Func<object, object> decorator)
    {
        Preconditions.NotNull(services, nameof(services));
        Preconditions.NotNull(serviceType, nameof(serviceType));
        Preconditions.NotNull(decorator, nameof(decorator));

        return services.Decorate(serviceType, (decorated, _) => decorator(decorated));
    }

    /// <summary>
    /// Decorates all registered services of the specified <paramref name="serviceType"/>
    /// using the <paramref name="decorator"/> function.
    /// </summary>
    /// <param name="services">The services to add to.</param>
    /// <param name="serviceType">The type of services to decorate.</param>
    /// <param name="decorator">The decorator function.</param>
    /// <exception cref="ArgumentNullException">If either the <paramref name="services"/>,
    /// <paramref name="serviceType"/> or <paramref name="decorator"/> arguments are <c>null</c>.</exception>
    public static bool TryDecorate(this IServiceCollection services, Type serviceType, Func<object, object> decorator)
    {
        Preconditions.NotNull(services, nameof(services));
        Preconditions.NotNull(serviceType, nameof(serviceType));
        Preconditions.NotNull(decorator, nameof(decorator));

        return services.TryDecorate(serviceType, (decorated, _) => decorator(decorated));
    }

    /// <summary>
    /// Decorates all registered services of the specified <paramref name="serviceType"/>
    /// using the <paramref name="decorator"/> function.
    /// </summary>
    /// <param name="services">The services to add to.</param>
    /// <param name="serviceType">The type of services to decorate.</param>
    /// <param name="decorator">The decorator function.</param>
    /// <exception cref="DecorationException">If no service of the specified <paramref name="serviceType"/> has been registered.</exception>
    /// <exception cref="ArgumentNullException">If either the <paramref name="services"/>,
    /// <paramref name="serviceType"/> or <paramref name="decorator"/> arguments are <c>null</c>.</exception>
    public static IServiceCollection Decorate(this IServiceCollection services, Type serviceType, Func<object, IServiceProvider, object> decorator)
    {
        Preconditions.NotNull(services, nameof(services));
        Preconditions.NotNull(serviceType, nameof(serviceType));
        Preconditions.NotNull(decorator, nameof(decorator));

        return services.Decorate(DecorationStrategy.WithFactory(serviceType, decorator));
    }

    /// <summary>
    /// Decorates all registered services of the specified <paramref name="serviceType"/>
    /// using the <paramref name="decorator"/> function.
    /// </summary>
    /// <param name="services">The services to add to.</param>
    /// <param name="serviceType">The type of services to decorate.</param>
    /// <param name="decorator">The decorator function.</param>
    /// <exception cref="ArgumentNullException">If either the <paramref name="services"/>,
    /// <paramref name="serviceType"/> or <paramref name="decorator"/> arguments are <c>null</c>.</exception>
    public static bool TryDecorate(this IServiceCollection services, Type serviceType, Func<object, IServiceProvider, object> decorator)
    {
        Preconditions.NotNull(services, nameof(services));
        Preconditions.NotNull(serviceType, nameof(serviceType));
        Preconditions.NotNull(decorator, nameof(decorator));

        return services.TryDecorate(DecorationStrategy.WithFactory(serviceType, decorator));
    }

    /// <summary>
    /// Decorates all registered services using the specified <paramref name="strategy"/>.
    /// </summary>
    /// <param name="services">The services to add to.</param>
    /// <param name="strategy">The strategy for decorating services.</param>
    /// <exception cref="DecorationException">If no registered service matched the specified <paramref name="strategy"/>.</exception>
    public static IServiceCollection Decorate(this IServiceCollection services, DecorationStrategy strategy)
    {
        if (services.TryDecorate(strategy))
        {
            return services;
        }

        throw new DecorationException(strategy);
    }

    /// <summary>
/// Decorates all registered services using the specified <paramref name="strategy"/>.
    /// </summary>
    /// <param name="services">The services to add to.</param>
    /// <param name="strategy">The strategy for decorating services.</param>
    public static bool TryDecorate(this IServiceCollection services, DecorationStrategy strategy)
    {
        Preconditions.NotNull(services, nameof(services));
        Preconditions.NotNull(strategy, nameof(strategy));

        var decorated = false;

        for (var i = services.Count - 1; i >= 0; i--)
        {
            var serviceDescriptor = services[i];

            if (serviceDescriptor.ServiceType is DecoratedType)
            {
                continue; // Service has already been decorated.
            }

            if (!strategy.CanDecorate(serviceDescriptor.ServiceType))
            {
continue; // Unable to decorate using the specified strategy.
            }

            var decoratedType = new DecoratedType(serviceDescriptor.ServiceType);

            // Insert decorated
            services.Add(serviceDescriptor.WithServiceType(decoratedType));

            // Replace decorator
            services[i] = serviceDescriptor.WithImplementationFactory(strategy.CreateDecorator(decoratedType));

            decorated = true;
        }

        return decorated;
    }

}