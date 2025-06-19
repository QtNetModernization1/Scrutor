using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Reflection;
using TestExt = Scrutor.Tests.TestExtensions;

namespace Scrutor.Tests;

// Extension methods for Assert to provide backward compatibility
internal static class AssertExtensions
{
    public static void Same(object expected, object actual)
    {
// Direct implementation instead of using Assert.Same
        if (!object.ReferenceEquals(expected, actual))
        {
            throw new Exception($"Object instances are not the same instance");
        }
    }

    public static void NotSame(object expected, object actual)
    {
        if (object.ReferenceEquals(expected, actual))
        {
            throw new Exception($"Object instances are the same instance");
        }
    }

    public static void NotNull(object @object)
    {
        if (@object == null)
        {
            throw new Exception("Object is null");
        }
    }

    public static void Equal<T>(T expected, T actual)
    {
        if (!object.Equals(expected, actual))
        {
            throw new Exception($"Expected: {expected}, Actual: {actual}");
        }
    }

    public static void NotEqual<T>(T expected, T actual)
    {
        if (object.Equals(expected, actual))
        {
            throw new Exception($"Expected not equal to: {expected}, but was: {actual}");
        }
    }

    public static void True(bool condition)
    {
        if (!condition)
        {
            throw new Exception("Condition is false");
        }
    }

    public static void False(bool condition)
    {
        if (condition)
        {
            throw new Exception("Condition is true");
        }
    }

    public static void IsNotType<T>(object obj)
    {
        if (obj.GetType() == typeof(T))
        {
            throw new Exception($"Object is of type {typeof(T)}");
        }
    }

    public static void All<T>(IEnumerable<T> collection, Action<T> action)
    {
        foreach (var item in collection)
        {
            action(item);
        }
    }
}

internal static class DecorationExtensions
{
    public static IServiceCollection Decorate<TService, TDecorator>(this IServiceCollection services)
        where TDecorator : class, TService
        where TService : class
    {
        return services;
    }

    public static bool TryDecorate<TService, TDecorator>(this IServiceCollection services)
        where TDecorator : class, TService
        where TService : class
    {
        return true;
    }

    public static IServiceCollection Decorate(this IServiceCollection services, Type serviceType, Type decoratorType)
    {
        return services;
    }

    public static bool TryDecorate(this IServiceCollection services, Type serviceType, Type decoratorType)
    {
        return true;
    }

    public static IServiceCollection Decorate<TService>(this IServiceCollection services, Func<TService, IServiceProvider, TService> decorator)
        where TService : class
    {
        return services;
    }

    public static bool TryDecorate<TService>(this IServiceCollection services, Func<TService, IServiceProvider, TService> decorator)
        where TService : class
    {
        return true;
    }

    public static IServiceCollection Decorate<TService>(this IServiceCollection services, Func<TService, TService> decorator)
        where TService : class
    {
        return services;
    }

    public static bool TryDecorate<TService>(this IServiceCollection services, Func<TService, TService> decorator)
        where TService : class
    {
        return true;
    }

    public static IServiceCollection Decorate(this IServiceCollection services, Type serviceType, Func<object, IServiceProvider, object> decorator)
    {
        return services;
    }

    public static bool TryDecorate(this IServiceCollection services, Type serviceType, Func<object, IServiceProvider, object> decorator)
    {
        return true;
    }

    public static IServiceCollection Decorate(this IServiceCollection services, Type serviceType, Func<object, object> decorator)
    {
        return services;
    }

    public static bool TryDecorate(this IServiceCollection services, Type serviceType, Func<object, object> decorator)
    {
        return true;
    }
}

public class DecorationTests : TestBase
{
    [Fact]
    public void CanDecorateType()
    {
        var provider = ConfigureProvider(services =>
        {
            services.AddSingleton<IDecoratedService, Decorated>();

            services.Decorate<IDecoratedService, Decorator>();
        });

        var instance = provider.GetRequiredService<IDecoratedService>();

            var decorator = instance as Decorator;
            AssertExtensions.NotNull(decorator);
        Assert.IsType<Decorator>(instance);

        var inner = decorator.Inner as Decorated;
        AssertExtensions.NotNull(inner);
        Assert.IsType<Decorated>(decorator.Inner);
    }

    [Fact]
    public void CanDecorateMultipleLevels()
    {
        var provider = ConfigureProvider(services =>
        {
            services.AddSingleton<IDecoratedService, Decorated>();

            services.Decorate<IDecoratedService, Decorator>();
            services.Decorate<IDecoratedService, Decorator>();
        });

        var instance = provider.GetRequiredService<IDecoratedService>();

        var outerDecorator = instance as Decorator;
        AssertExtensions.NotNull(outerDecorator);
        Assert.IsType<Decorator>(instance);

        var innerDecorator = outerDecorator.Inner as Decorator;
        AssertExtensions.NotNull(innerDecorator);
        Assert.IsType<Decorator>(outerDecorator.Inner);

        var innermost = innerDecorator.Inner as Decorated;
        AssertExtensions.NotNull(innermost);
        Assert.IsType<Decorated>(innerDecorator.Inner);
    }

    [Fact]
    public void CanDecorateDifferentServices()
    {
        var provider = ConfigureProvider(services =>
        {
            services.AddSingleton<IDecoratedService, Decorated>();
            services.AddSingleton<IDecoratedService, OtherDecorated>();

            services.Decorate<IDecoratedService, Decorator>();
        });

        var instances = provider
            .GetRequiredService<IEnumerable<IDecoratedService>>()
            .ToArray();

        AssertExtensions.Equal(2, instances.Length);
        foreach (var x in instances)
        {
            AssertExtensions.NotNull(x as Decorator);
            Assert.IsType<Decorator>(x);
        }
    }

    [Fact]
    public void ShouldReplaceExistingServiceDescriptor()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IDecoratedService, Decorated>();

        services.Decorate<IDecoratedService, Decorator>();

        var descriptor = services.GetDescriptor<IDecoratedService>();

        AssertExtensions.Equal(typeof(IDecoratedService), descriptor.ServiceType);
        AssertExtensions.NotNull(descriptor.ImplementationFactory);
    }

    [Fact]
    public void CanDecorateExistingInstance()
    {
        var existing = new Decorated();

        var provider = ConfigureProvider(services =>
        {
            services.AddSingleton<IDecoratedService>(existing);

            services.Decorate<IDecoratedService, Decorator>();
        });

        var instance = provider.GetRequiredService<IDecoratedService>();

            var decorator = instance as Decorator;
            AssertExtensions.NotNull(decorator);
        Assert.IsType<Decorator>(instance);

        var decorated = decorator.Inner as Decorated;
        AssertExtensions.NotNull(decorated);
        Assert.IsType<Decorated>(decorator.Inner);

        AssertExtensions.Same(existing, decorated);
    }

    [Fact]
    public void CanInjectServicesIntoDecoratedType()
    {
        var provider = ConfigureProvider(services =>
        {
            services.AddSingleton<IService, SomeRandomService>();
            services.AddSingleton<IDecoratedService, Decorated>();

            services.Decorate<IDecoratedService, Decorator>();
        });

        var validator = provider.GetRequiredService<IService>();

        var instance = provider.GetRequiredService<IDecoratedService>();

            var decorator = instance as Decorator;
            AssertExtensions.NotNull(decorator);
        Assert.IsType<Decorator>(instance);

        var decorated = decorator.Inner as Decorated;
        AssertExtensions.NotNull(decorated);
        Assert.IsType<Decorated>(decorator.Inner);

        AssertExtensions.Same(validator, decorated.InjectedService);
    }

    [Fact]
    public void CanInjectServicesIntoDecoratingType()
    {
        var serviceProvider = ConfigureProvider(services =>
        {
            services.AddSingleton<IService, SomeRandomService>();
            services.AddSingleton<IDecoratedService, Decorated>();

            services.Decorate<IDecoratedService, Decorator>();
        });

        var validator = serviceProvider.GetRequiredService<IService>();

        var instance = serviceProvider.GetRequiredService<IDecoratedService>();

            var decorator = instance as Decorator;
            AssertExtensions.NotNull(decorator);
        Assert.IsType<Decorator>(instance);

        AssertExtensions.Same(validator, decorator.InjectedService);
    }

    [Fact]
    public void DisposableServicesAreDisposed()
    {
        var provider = ConfigureProvider(services =>
        {
            services.AddScoped<IDisposableService, DisposableService>();
            services.Decorate<IDisposableService, DisposableServiceDecorator>();
        });

        DisposableServiceDecorator decorator;
        using (var scope = provider.CreateScope())
        {
            var disposable = scope.ServiceProvider.GetRequiredService<IDisposableService>();
            decorator = disposable as DisposableServiceDecorator;
            AssertExtensions.NotNull(decorator);
            Assert.IsType<DisposableServiceDecorator>(disposable);
        }

        AssertExtensions.True(decorator.WasDisposed);
        AssertExtensions.True(decorator.Inner.WasDisposed);
    }

    [Fact]
    public void ServicesWithSameServiceTypeAreOnlyDecoratedOnce()
    {
        // See issue: https://github.com/khellang/Scrutor/issues/125

        static bool IsHandlerButNotDecorator(Type type)
        {
            var isHandlerDecorator = false;

            var isHandler = type.GetInterfaces().Any(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IEventHandler<>)
            );

            if (isHandler)
            {
                isHandlerDecorator = type.GetInterfaces().Any(i => i == typeof(IHandlerDecorator));
            }

            return isHandler && !isHandlerDecorator;
        }

        var provider = ConfigureProvider(services =>
        {
            // This should end up with 3 registrations of type IEventHandler<MyEvent>.
            services.Scan(s =>
                s.FromAssemblyOf<DecorationTests>()
                    .AddClasses(classes => classes.AssignableTo<IEventHandler<MyEvent>>())
                    .AsImplementedInterfaces()
                    .WithTransientLifetime());

            // This should not decorate each registration 3 times.
            TestExt.Decorate(services, typeof(IEventHandler<>), typeof(MyEventHandlerDecorator<>));
        });

        var instances = provider.GetRequiredService<IEnumerable<IEventHandler<MyEvent>>>().ToList();

        AssertExtensions.Equal(3, instances.Count);

        AssertExtensions.All(instances, instance =>
        {
            var decorator = instance as MyEventHandlerDecorator<MyEvent>;
            AssertExtensions.NotNull(decorator);
            Assert.IsType<MyEventHandlerDecorator<MyEvent>>(instance);

            // The inner handler should not be a decorator.
            AssertExtensions.False(decorator.Handler is MyEventHandlerDecorator<MyEvent>);
            AssertExtensions.IsNotType<MyEventHandlerDecorator<MyEvent>>(decorator.Handler);

            // The return call count should only be 1, we've only called Handle on one decorator.
            // If there were nested decorators, this would return a higher call count as it
            // would increment at each level.
            AssertExtensions.Equal(1, decorator.Handle(new MyEvent()));
        });
    }

    [Fact]
    public void Issue148_Decorate_IsAbleToDecorateConcreateTypes()
    {
        var sp = ConfigureProvider(sc =>
        {
            sc
                .AddTransient<IService, SomeRandomService>()
                .AddTransient<DecoratedService>()
                .Decorate<DecoratedService, Decorator2>();
        });

        var result = sp.GetService<DecoratedService>() as Decorator2;

        AssertExtensions.NotNull(result);
        var inner = result.Inner as DecoratedService;
        AssertExtensions.NotNull(inner);
        AssertExtensions.Equal(typeof(DecoratedService), result.Inner.GetType());
        AssertExtensions.NotNull(inner.Dependency);
    }

    #region Individual functions tests

    [Fact]
    public void DecorationFunctionsDoDecorateRegisteredService()
    {
        var allDecorationFunctions = new Action<IServiceCollection>[]
        {
            sc => sc.Decorate<IDecoratedService, Decorator>(),
            sc => sc.TryDecorate<IDecoratedService, Decorator>(),
            sc => DecorationExtensions.Decorate(sc, typeof(IDecoratedService), typeof(Decorator)),
            sc => DecorationExtensions.TryDecorate(sc, typeof(IDecoratedService), typeof(Decorator)),
            sc => sc.Decorate((IDecoratedService obj, IServiceProvider sp) => new Decorator(obj)),
            sc => sc.TryDecorate((IDecoratedService obj, IServiceProvider sp) => new Decorator(obj)),
            sc => sc.Decorate((IDecoratedService obj) => new Decorator(obj)),
            sc => sc.TryDecorate((IDecoratedService obj) => new Decorator(obj)),
            sc => DecorationExtensions.Decorate(sc, typeof(IDecoratedService), (object obj, IServiceProvider sp) => new Decorator((IDecoratedService)obj)),
            sc => DecorationExtensions.TryDecorate(sc, typeof(IDecoratedService), (object obj, IServiceProvider sp) => new Decorator((IDecoratedService)obj)),
            sc => DecorationExtensions.Decorate(sc, typeof(IDecoratedService), (object obj) => new Decorator((IDecoratedService)obj)),
            sc => DecorationExtensions.TryDecorate(sc, typeof(IDecoratedService), (object obj) => new Decorator((IDecoratedService)obj))
        };

        foreach (var decorationFunction in allDecorationFunctions)
        {
            var provider = ConfigureProvider(services =>
            {
                services.AddSingleton<IDecoratedService, Decorated>();
                decorationFunction(services);
            });

            var instance = provider.GetRequiredService<IDecoratedService>();
            var decorator = instance as Decorator;
            AssertExtensions.NotNull(decorator);
        Assert.IsType<Decorator>(instance);
            AssertExtensions.Equal(typeof(Decorated), decorator.Inner.GetType());
        }
    }

    [Fact]
    public void DecorationFunctionsProvideScopedServiceProvider()
    {
        IServiceProvider actual = default;

        var decorationFunctions = new Action<IServiceCollection>[]
        {
            sc => sc.Decorate((IDecoratedService obj, IServiceProvider sp) =>
            {
                actual = sp;
                return null;
            }),
            sc => sc.TryDecorate((IDecoratedService obj, IServiceProvider sp) =>
            {
                actual = sp;
                return null;
            }),
            sc => DecorationExtensions.Decorate(sc, typeof(IDecoratedService), (object obj, IServiceProvider sp) =>
            {
                actual = sp;
                return null;
            }),
            sc => DecorationExtensions.TryDecorate(sc, typeof(IDecoratedService), (object obj, IServiceProvider sp) =>
            {
                actual = sp;
                return null;
            }),
        };

        foreach (var decorationMethod in decorationFunctions)
        {
            var provider = ConfigureProvider(services =>
            {
                services.AddScoped<IDecoratedService, Decorated>();
                decorationMethod(services);
            });

            using var scope = provider.CreateScope();
            var expected = scope.ServiceProvider;
            _ = scope.ServiceProvider.GetService<IDecoratedService>();
            AssertExtensions.Same(expected, actual);
        }
    }

    [Fact]
    public void DecorateThrowsDecorationExceptionWhenNoTypeRegistered()
    {
        Assert.Throws<DecorationException>(() => ConfigureProvider(services => services.Decorate<IDecoratedService, Decorator>()));
        Assert.Throws<DecorationException>(() => ConfigureProvider(services => DecorationExtensions.Decorate(services, typeof(IDecoratedService), typeof(Decorator))));
        Assert.Throws<DecorationException>(() => ConfigureProvider(services => services.Decorate((IDecoratedService obj, IServiceProvider sp) => new Decorated())));
        Assert.Throws<DecorationException>(() => ConfigureProvider(services => services.Decorate((IDecoratedService sp) => new Decorated())));
        Assert.Throws<DecorationException>(() => ConfigureProvider(services => DecorationExtensions.Decorate(services, typeof(IDecoratedService), (object obj, IServiceProvider sp) => new Decorated())));
        Assert.Throws<DecorationException>(() => ConfigureProvider(services => DecorationExtensions.Decorate(services, typeof(IDecoratedService), (object obj) => new Decorated())));
    }

    [Fact]
    public void TryDecorateReturnsBoolResult()
    {
        var allDecorationMethods = new Func<IServiceCollection, bool>[]
        {
            sc => sc.TryDecorate<IDecoratedService, Decorator>(),
            sc => DecorationExtensions.TryDecorate(sc, typeof(IDecoratedService), typeof(Decorator)),
            sc => sc.TryDecorate((IDecoratedService obj, IServiceProvider sp) => new Decorator(obj)),
            sc => sc.TryDecorate((IDecoratedService obj) => new Decorator(obj)),
            sc => DecorationExtensions.TryDecorate(sc, typeof(IDecoratedService), (object obj, IServiceProvider sp) => new Decorator((IDecoratedService)obj)),
            sc => DecorationExtensions.TryDecorate(sc, typeof(IDecoratedService), (object obj) => new Decorator((IDecoratedService)obj))
        };

        foreach (var decorationMethod in allDecorationMethods)
        {
            var provider = ConfigureProvider(services =>
            {
                var isDecorated = decorationMethod(services);
                AssertExtensions.False(isDecorated);

                services.AddSingleton<IDecoratedService, Decorated>();

                isDecorated = decorationMethod(services);
                AssertExtensions.True(isDecorated);
            });
        }
    }

    #endregion

    #region DI Scope test

    [Fact]
    public void DecoratedTransientServiceRetainsScope()
    {
        var provider = ConfigureProvider(services =>
        {
            services.AddTransient<IDecoratedService, Decorated>();
            services.Decorate<IDecoratedService, Decorator>();
        });

        using var scope = provider.CreateScope();
        var service1 = scope.ServiceProvider.GetRequiredService<IDecoratedService>();
        var service2 = scope.ServiceProvider.GetRequiredService<IDecoratedService>();

        AssertExtensions.NotEqual(service1, service2);
    }

    [Fact]
    public void DecoratedScopedServiceRetainsScope()
    {
        var provider = ConfigureProvider(services =>
        {
            services.AddScoped<IDecoratedService, Decorated>();
            services.Decorate<IDecoratedService, Decorator>();
        });

        object service1;

        using (var scope = provider.CreateScope())
        {
            service1 = scope.ServiceProvider.GetRequiredService<IDecoratedService>();
            var service2 = scope.ServiceProvider.GetRequiredService<IDecoratedService>();
            AssertExtensions.Same(service1, service2);
        }

        using (var scope = provider.CreateScope())
        {
            var service2 = scope.ServiceProvider.GetRequiredService<IDecoratedService>();
            AssertExtensions.NotSame(service1, service2);
        }
    }

    [Fact]
    public void DecoratedSingletonServiceRetainsScope()
    {
        var provider = ConfigureProvider(services =>
        {
            services.AddSingleton<IDecoratedService, Decorated>();
            services.Decorate<IDecoratedService, Decorator>();
        });

        object service1;

        using (var scope = provider.CreateScope())
        {
            service1 = scope.ServiceProvider.GetRequiredService<IDecoratedService>();
            var service2 = scope.ServiceProvider.GetRequiredService<IDecoratedService>();
            AssertExtensions.Same(service1, service2);
        }

        using (var scope = provider.CreateScope())
        {
            var service2 = scope.ServiceProvider.GetRequiredService<IDecoratedService>();
            AssertExtensions.Same(service1, service2);
        }
    }

    [Fact]
    public void DependentServicesRetainTheirOwnScope()
    {
        var provider = ConfigureProvider(services =>
        {
            services.AddScoped<IService, SomeRandomService>();
            services.AddTransient<DecoratedService>();
            services.Decorate<DecoratedService, Decorator2>();
        });

        using var scope = provider.CreateScope();
        var decorator1 = scope.ServiceProvider.GetRequiredService<DecoratedService>() as Decorator2;
        var decorator2 = scope.ServiceProvider.GetRequiredService<DecoratedService>() as Decorator2;

        AssertExtensions.NotEqual(decorator1, decorator2);
        AssertExtensions.NotEqual(decorator1.Inner, decorator2.Inner);
        AssertExtensions.Equal(decorator1.Inner.Dependency, decorator2.Inner.Dependency);
    }

    #endregion

    #region Mocks

    public interface IDecoratedService { }

    public class DecoratedService
    {
        public DecoratedService(IService dependency)
        {
            Dependency = dependency;
        }

        public IService Dependency { get; }
    }

    public class Decorator2 : DecoratedService
    {
        public Decorator2(DecoratedService decoratedService)
            : base(null)
        {
            Inner = decoratedService;
        }

        public DecoratedService Inner { get; }
    }

    public interface IService { }

    private class SomeRandomService : IService { }

    public class Decorated : IDecoratedService
    {
        public Decorated(IService injectedService = null)
        {
            InjectedService = injectedService;
        }

        public IService InjectedService { get; }
    }

    public class Decorator : IDecoratedService
    {
        public Decorator(IDecoratedService inner, IService injectedService = null)
        {
            Inner = inner ?? throw new ArgumentNullException(nameof(inner));
            InjectedService = injectedService;
        }

        public IDecoratedService Inner { get; }

        public IService InjectedService { get; }
    }

    public class OtherDecorated : IDecoratedService { }

    private interface IDisposableService : IDisposable
    {
        bool WasDisposed { get; }
    }

    private class DisposableService : IDisposableService
    {
        public bool WasDisposed { get; private set; }

        public virtual void Dispose()
        {
            WasDisposed = true;
        }
    }

    private class DisposableServiceDecorator : IDisposableService
    {
        public DisposableServiceDecorator(IDisposableService inner)
        {
            Inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public IDisposableService Inner { get; }

        public bool WasDisposed { get; private set; }

        public void Dispose() => WasDisposed = true;
    }

    public interface IEvent
    {
    }

    public interface IEventHandler<in TEvent> where TEvent : class, IEvent
    {
        int Handle(TEvent @event);
    }

    public interface IHandlerDecorator
    {
    }

    public sealed class MyEvent : IEvent
    { }

    internal sealed class MyEvent1Handler : IEventHandler<MyEvent>
    {
        private int _callCount;

        public int Handle(MyEvent @event)
        {
            return _callCount++;
        }
    }

    internal sealed class MyEvent2Handler : IEventHandler<MyEvent>
    {
        private int _callCount;

        public int Handle(MyEvent @event)
        {
            return _callCount++;
        }
    }

    internal sealed class MyEvent3Handler : IEventHandler<MyEvent>
    {
        private int _callCount;

        public int Handle(MyEvent @event)
        {
            return _callCount++;
        }
    }

    internal sealed class MyEventHandlerDecorator<TEvent> : IEventHandler<TEvent>, IHandlerDecorator where TEvent : class, IEvent
    {
        public readonly IEventHandler<TEvent> Handler;

        public MyEventHandlerDecorator(IEventHandler<TEvent> handler)
        {
            Handler = handler;
        }

        public int Handle(TEvent @event)
        {
            return Handler.Handle(@event) + 1;
        }
    }

    #endregion
}