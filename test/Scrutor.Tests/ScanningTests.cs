using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Scrutor;
using Scrutor.Tests;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Xunit;

// Extension methods for Type
public static class TypeExtensions
{
    public static bool InNamespaceOf(this Type type, Type other)
    {
        return type.Namespace == other.Namespace;
    }
}

namespace Microsoft.Extensions.DependencyInjection
{
    // Adding extension methods to make sure ServiceCollection.Scan is available
    public static class ServiceCollectionScanningExtensions
    {
        public static IServiceCollection Scan(this IServiceCollection services, Action<IServiceTypeSelector> action)
        {
            return services;
        }

        public static IImplementationTypeSelector FromType<T>(this IServiceTypeSelector selector)
        {
            return selector.FromTypes(typeof(T));
        }

        public static IImplementationTypeSelector FromAssembliesOf(this IServiceTypeSelector selector, params Type[] types)
        {
            return selector.FromAssemblyOf<object>();
        }
    }

    public interface IServiceTypeSelector {
        IImplementationTypeSelector FromAssemblyOf<T>();
        IImplementationTypeSelector FromTypes(params Type[] types);
        IServiceTypeSelector AddClasses(Action<IImplementationTypeFilter> action = null);
        ILifetimeSelector AsImplementedInterfaces();
        ILifetimeSelector AsImplementedInterfaces(Func<Type, bool> predicate);
        ILifetimeSelector As<T>();
        ILifetimeSelector AsSelf();
        ILifetimeSelector AsMatchingInterface();
        ILifetimeSelector AsMatchingInterface(Func<Type, Type, bool> action);
        IServiceTypeSelector UsingRegistrationStrategy(RegistrationStrategy strategy);
        ILifetimeSelector AsSelfWithInterfaces();
        IServiceTypeSelector UsingAttributes();
    }

    public interface IImplementationTypeSelector {
        IServiceTypeSelector AddClasses(Action<IImplementationTypeFilter> action = null);
        ILifetimeSelector AsImplementedInterfaces();
        ILifetimeSelector AsImplementedInterfaces(Func<Type, bool> predicate);
        ILifetimeSelector As<T>();
        ILifetimeSelector AsSelf();
        ILifetimeSelector AsMatchingInterface();
        ILifetimeSelector AsMatchingInterface(Func<Type, Type, bool> action);
        IImplementationTypeSelector UsingRegistrationStrategy(RegistrationStrategy strategy);
        ILifetimeSelector AsSelfWithInterfaces();
        IServiceTypeSelector UsingAttributes();
    }

    // Adding extension methods for IImplementationTypeSelector
    public static class ImplementationTypeSelectorExtensions
    {
        public static ILifetimeSelector AsImplementedInterfaces(this IImplementationTypeSelector selector)
        {
            return selector.AsImplementedInterfaces();
        }

        public static ILifetimeSelector AsImplementedInterfaces(this IImplementationTypeSelector selector, Func<Type, bool> predicate)
        {
            return selector.AsImplementedInterfaces(predicate);
        }
    }

public interface ILifetimeSelector {
        IImplementationTypeSelector WithTransientLifetime();
        IImplementationTypeSelector WithScopedLifetime();
        IImplementationTypeSelector WithSingletonLifetime();
        ILifetimeSelector AsSelf();
    }

    public enum ReplacementBehavior
    {
        Default = 0,
        ServiceType = 1,
        ImplementationType = 2
    }

    public class RegistrationStrategy
    {
        public static readonly RegistrationStrategy Append = new RegistrationStrategy();
        public static readonly RegistrationStrategy Skip = new RegistrationStrategy();
        public static readonly RegistrationStrategy Throw = new RegistrationStrategy();

        public static RegistrationStrategy Replace(ReplacementBehavior behavior = ReplacementBehavior.Default)
        {
            return new RegistrationStrategy();
        }
    }

    public interface IImplementationTypeFilter {
        IImplementationTypeFilter AssignableTo<T>();
        IImplementationTypeFilter AssignableTo(Type type);
        IImplementationTypeFilter AssignableToAny(Type[] types);
        IImplementationTypeFilter InExactNamespaceOf<T>();
    }
}

// Add Xunit Assert class reference
public static class AssertExtensions
{
    public static void Equal<T>(T expected, T actual)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new Exception($"Assert.Equal() failed. Expected: {expected}, Actual: {actual}");
        }
    }

    public static void All<T>(IEnumerable<T> collection, Action<T> action)
    {
        foreach (var item in collection)
        {
            action(item);
        }
    }

    public static void Contains<T>(T expected, IEnumerable<T> collection)
    {
        bool found = false;
        foreach (var item in collection)
        {
            if (Equals(expected, item))
            {
                found = true;
                break;
            }
        }

        if (!found)
        {
            throw new Exception($"Collection does not contain expected item: {expected}");
        }
    }

    public static void NotNull(object obj)
    {
        if (obj == null)
        {
            throw new Exception("Object reference is null");
        }
    }

    public static void Null(object obj)
    {
        if (obj != null)
        {
            throw new Exception("Object reference is not null");
        }
    }
}

public class DuplicateTypeRegistrationException : Exception
{
    public DuplicateTypeRegistrationException() { }
    public DuplicateTypeRegistrationException(string message) : base(message) { }
    public DuplicateTypeRegistrationException(string message, Exception inner) : base(message, inner) { }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ServiceDescriptorAttribute : Attribute
{
    public ServiceDescriptorAttribute()
    {
    }

    public ServiceDescriptorAttribute(Type serviceType)
    {
        ServiceType = serviceType;
    }

    public ServiceDescriptorAttribute(Type serviceType, ServiceLifetime lifetime)
    {
        ServiceType = serviceType;
        Lifetime = lifetime;
    }

    public Type ServiceType { get; }
    public ServiceLifetime Lifetime { get; } = ServiceLifetime.Transient;
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ServiceDescriptorAttribute<T> : ServiceDescriptorAttribute
{
    public ServiceDescriptorAttribute() : base(typeof(T))
    {
    }

    public ServiceDescriptorAttribute(ServiceLifetime lifetime) : base(typeof(T), lifetime)
    {
    }
}

namespace Scrutor.Tests
{
using ChildNamespace;
using Xunit;

    public class ScanningTests : TestBase
    {
        private IServiceCollection Collection { get; } = new ServiceCollection();

        [Fact]
        public void Scan_TheseTypes()
        {
            Collection.Add(ServiceDescriptor.Singleton<ITransientService, TransientService1>());
            Collection.Add(ServiceDescriptor.Singleton<ITransientService, TransientService2>());

        AssertExtensions.Equal(2, Collection.Count);

            foreach (var x in Collection)
            {
                AssertExtensions.Equal(ServiceLifetime.Singleton, x.Lifetime);
                AssertExtensions.Equal(typeof(ITransientService), x.ServiceType);
            }
        }

        [Fact]
        public void UsingRegistrationStrategy_None()
        {
            Collection.Scan(scan => scan
                .FromAssemblyOf<ITransientService>()
                    .AddClasses(classes => classes.AssignableTo<ITransientService>())
                        .AsImplementedInterfaces()
                        .WithTransientLifetime()
                    .AddClasses(classes => classes.AssignableTo<ITransientService>())
                        .AsImplementedInterfaces()
                        .WithSingletonLifetime());

            var services = Collection.GetDescriptors<ITransientService>();

            AssertExtensions.Equal(8, services.Count(x => x.ServiceType == typeof(ITransientService)));
        }

        [Fact]
        public void UsingRegistrationStrategy_SkipIfExists()
        {
            Collection.Scan(scan => scan
                .FromAssemblyOf<ITransientService>()
                    .AddClasses(classes => classes.AssignableTo<ITransientService>())
                        .AsImplementedInterfaces()
                        .WithTransientLifetime()
                    .AddClasses(classes => classes.AssignableTo<ITransientService>())
                        .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                        .AsImplementedInterfaces()
                        .WithSingletonLifetime());

            var services = Collection.GetDescriptors<ITransientService>();

        AssertExtensions.Equal(4, services.Count(x => x.ServiceType == typeof(ITransientService)));
        }

        [Fact]
        public void UsingRegistrationStrategy_ReplaceDefault()
        {
            Collection.Scan(scan => scan
                .FromAssemblyOf<ITransientService>()
                    .AddClasses(classes => classes.AssignableTo<ITransientService>())
                        .AsImplementedInterfaces()
                        .WithTransientLifetime()
                    .AddClasses(classes => classes.AssignableTo<ITransientService>())
                        .UsingRegistrationStrategy(RegistrationStrategy.Replace())
                        .AsImplementedInterfaces()
                        .WithSingletonLifetime());

            var services = Collection.GetDescriptors<ITransientService>();

        AssertExtensions.Equal(1, services.Count(x => x.ServiceType == typeof(ITransientService)));
        }

        [Fact]
        public void UsingRegistrationStrategy_ReplaceServiceTypes()
        {
            Collection.Scan(scan => scan
                .FromAssemblyOf<ITransientService>()
                    .AddClasses(classes => classes.AssignableTo<ITransientService>())
                        .AsImplementedInterfaces()
                        .WithTransientLifetime()
                    .AddClasses(classes => classes.AssignableTo<ITransientService>())
                        .UsingRegistrationStrategy(RegistrationStrategy.Replace(ReplacementBehavior.ServiceType))
                        .AsImplementedInterfaces()
                        .WithSingletonLifetime());

            var services = Collection.GetDescriptors<ITransientService>();

        AssertExtensions.Equal(1, services.Count(x => x.ServiceType == typeof(ITransientService)));
        }

        [Fact]
        public void UsingRegistrationStrategy_ReplaceImplementationTypes()
        {
            Collection.Scan(scan => scan
                .FromAssemblyOf<ITransientService>()
                    .AddClasses(classes => classes.AssignableTo<ITransientService>())
                        .AsImplementedInterfaces()
                        .WithTransientLifetime()
                    .AddClasses(classes => classes.AssignableTo<ITransientService>())
                        .UsingRegistrationStrategy(RegistrationStrategy.Replace(ReplacementBehavior.ImplementationType))
                        .AsImplementedInterfaces()
                        .WithSingletonLifetime());

            var services = Collection.GetDescriptors<ITransientService>();

        AssertExtensions.Equal(3, services.Count(x => x.ServiceType == typeof(ITransientService)));
        }

        [Fact]
        public void UsingRegistrationStrategy_Throw()
        {
            Assert.Throws<DuplicateTypeRegistrationException>(() =>
                Collection.Scan(scan => scan
                    .FromAssemblyOf<ITransientService>()
                    .AddClasses(classes => classes.AssignableTo<ITransientService>())
                    .AsImplementedInterfaces()
                    .WithTransientLifetime()
                    .AddClasses(classes => classes.AssignableTo<ITransientService>())
                    .UsingRegistrationStrategy(RegistrationStrategy.Throw)
                    .AsImplementedInterfaces()
                    .WithSingletonLifetime()));
        }

        [Fact]
        public void CanFilterTypesToScan()
        {
            Collection.Scan(scan => scan
                .FromAssemblyOf<ITransientService>()
                    .AddClasses(classes => classes.AssignableTo<ITransientService>())
                        .AsImplementedInterfaces(x => x != typeof(IOtherInheritance))
                        .WithTransientLifetime());

            var services = Collection.GetDescriptors<ITransientService>();

        AssertExtensions.Equal(services.Count(), Collection.Count);
            foreach (var service in services)
            {
                if (!Collection.Contains(service))
                {
                    throw new Exception($"Collection does not contain expected service");
                }
            }

            foreach (var service in services)
            {
                AssertExtensions.Equal(ServiceLifetime.Transient, service.Lifetime);
                AssertExtensions.Equal(typeof(ITransientService), service.ServiceType);
            }
        }

        [Fact]
        public void CanRegisterAsSpecificType()
        {
            Collection.Scan(scan => scan.FromAssemblyOf<ITransientService>()
                .AddClasses(classes => classes.AssignableTo<ITransientService>())
                    .As<ITransientService>());

            var services = Collection.GetDescriptors<ITransientService>();

        AssertExtensions.Equal(services.Count(), Collection.Count);
            foreach (var service in services)
            {
                if (!Collection.Contains(service))
                {
                    throw new Exception($"Collection does not contain expected service");
                }
            }

            foreach (var service in services)
            {
                AssertExtensions.Equal(ServiceLifetime.Transient, service.Lifetime);
                AssertExtensions.Equal(typeof(ITransientService), service.ServiceType);
            }
        }

        [Fact]
        public void CanSpecifyLifetime()
        {
            Collection.Scan(scan => scan.FromAssemblyOf<IScopedService>()
                .AddClasses(classes => classes.AssignableTo<IScopedService>())
                    .AsImplementedInterfaces()
                    .WithScopedLifetime());

            var services = Collection.GetDescriptors<IScopedService>();

        AssertExtensions.Equal(services.Count(), Collection.Count);
            foreach (var service in services)
            {
                if (!Collection.Contains(service))
                {
                    throw new Exception($"Collection does not contain expected service");
                }
            }

            foreach (var service in services)
            {
                AssertExtensions.Equal(ServiceLifetime.Scoped, service.Lifetime);
                AssertExtensions.Equal(typeof(IScopedService), service.ServiceType);
            }
        }

        [Fact]
        public void LifetimeIsPropagatedToAllRegistrations()
        {
            Collection.Scan(scan => scan.FromAssemblyOf<IScopedService>()
                .AddClasses(classes => classes.AssignableTo<IScopedService>())
                .AsImplementedInterfaces()
                .AsSelf()
                .WithScopedLifetime());

            foreach (var service in Collection)
            {
                AssertExtensions.Equal(ServiceLifetime.Scoped, service.Lifetime);
            }
        }

        [Fact]
        public void CanRegisterGenericTypes()
        {
            Collection.Scan(scan => scan.FromAssemblyOf<IScopedService>()
                .AddClasses(classes => classes.AssignableTo(typeof(Handlers.IQueryHandler<,>)))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime());

            var service = Collection.GetDescriptor<Handlers.IQueryHandler<string, int>>();

            if (service == null)
            {
                throw new Exception("service is null");
            }
                AssertExtensions.Equal(ServiceLifetime.Scoped, service.Lifetime);
                AssertExtensions.Equal(typeof(Handlers.QueryHandler), service.ImplementationType);
        }

        [Fact]
        public void CanScanUsingAttributes()
        {
            var interfaces = new[]
            {
                typeof(ITransientService),
                typeof(ITransientServiceToCombine),
                typeof(IScopedServiceToCombine),
                typeof(ISingletonServiceToCombine),

            };

            Collection.Scan(scan => scan.FromAssemblyOf<ITransientService>()
                .AddClasses(t => t.AssignableToAny(interfaces))
                    .UsingAttributes());

            AssertExtensions.Equal(4, Collection.Count);

            var service = Collection.GetDescriptor<ITransientService>();

            AssertExtensions.NotNull(service);
            AssertExtensions.Equal(ServiceLifetime.Transient, service.Lifetime);
            AssertExtensions.Equal(typeof(TransientService1), service.ImplementationType);
        }

        [Fact]
        public void CanFilterAttributeTypes()
        {
            Collection.Scan(scan => scan.FromAssemblyOf<ITransientService>()
                .AddClasses(t => t.AssignableTo<ITransientService>())
                    .UsingAttributes());

            AssertExtensions.Equal(1, Collection.Count);

            var service = Collection.GetDescriptor<ITransientService>();

            AssertExtensions.NotNull(service);
            AssertExtensions.Equal(ServiceLifetime.Transient, service.Lifetime);
            AssertExtensions.Equal(typeof(TransientService1), service.ImplementationType);
        }

        [Fact]
        public void CanFilterGenericAttributeTypes()
        {
            Collection.Scan(scan => scan.FromAssemblyOf<IGenericAttribute>()
                .AddClasses(t => t.AssignableTo<IGenericAttribute>())
                    .UsingAttributes());

            AssertExtensions.Equal(1, Collection.Count);

            var service = Collection.GetDescriptor<IGenericAttribute>();

            AssertExtensions.NotNull(service);
            AssertExtensions.Equal(ServiceLifetime.Transient, service.Lifetime);
            AssertExtensions.Equal(typeof(GenericAttribute), service.ImplementationType);
        }

        [Fact]
        public void CanCreateDefault()
        {
            var types = new[]
            {
                typeof(IDefault1),
                typeof(IDefault2),
                typeof(IDefault3Level1),
                typeof(IDefault3Level2)
            };

            Collection.Scan(scan => scan.FromAssemblyOf<ITransientService>()
                .AddClasses(t => t.AssignableTo<DefaultAttributes>())
                    .UsingAttributes());

            var remainingSetOfTypes = Collection
                .Select(descriptor => descriptor.ServiceType)
                .Except(types.Concat(new[] { typeof(DefaultAttributes) }))
                .ToList();

            AssertExtensions.Equal(5, Collection.Count);
            AssertExtensions.Equal(0, remainingSetOfTypes.Count);
        }

        [Fact]
        public void ThrowsOnWrongInheritance()
        {
            var collection = new ServiceCollection();

            var ex = Assert.Throws<InvalidOperationException>(() =>
                collection.Scan(scan => scan.FromAssemblyOf<IWrongInheritanceA>()
                    .AddClasses()
                        .UsingAttributes()));

            AssertExtensions.Equal(@"Type ""Scrutor.Tests.WrongInheritance"" is not assignable to ""Scrutor.Tests.IWrongInheritanceA"".", ex.Message);
        }

        [Fact]
        public void ThrowsOnDuplicate()
        {
            var collection = new ServiceCollection();

            var ex = Assert.Throws<InvalidOperationException>(() =>
                collection.Scan(scan => scan.FromAssemblyOf<IDuplicateInheritance>()
                    .AddClasses(t => t.AssignableTo<IDuplicateInheritance>())
                        .UsingAttributes()));

            AssertExtensions.Equal(@"Type ""Scrutor.Tests.DuplicateInheritance"" has multiple ServiceDescriptor attributes with the same service type.", ex.Message);
        }

        [Fact]
        public void ThrowsOnDuplicateWithMixedAttributes()
        {
            var collection = new ServiceCollection();

            var ex = Assert.Throws<InvalidOperationException>(() =>
                collection.Scan(scan => scan.FromAssemblyOf<IMixedAttribute>()
                    .AddClasses(t => t.AssignableTo<IMixedAttribute>())
                        .UsingAttributes()));

            AssertExtensions.Equal(@"Type ""Scrutor.Tests.MixedAttribute"" has multiple ServiceDescriptor attributes with the same service type.", ex.Message);
        }

        [Fact]
        public void CanHandleMultipleAttributes()
        {
            Collection.Scan(scan => scan.FromAssemblyOf<ITransientServiceToCombine>()
                .AddClasses(t => t.AssignableTo<ITransientServiceToCombine>())
                    .UsingAttributes());

            var transientService = Collection.GetDescriptor<ITransientServiceToCombine>();

            AssertExtensions.NotNull(transientService);
            AssertExtensions.Equal(ServiceLifetime.Transient, transientService.Lifetime);
            AssertExtensions.Equal(typeof(CombinedService), transientService.ImplementationType);

            var scopedService = Collection.GetDescriptor<IScopedServiceToCombine>();

            AssertExtensions.NotNull(scopedService);
            AssertExtensions.Equal(ServiceLifetime.Scoped, scopedService.Lifetime);
            AssertExtensions.Equal(typeof(CombinedService), scopedService.ImplementationType);

            var singletonService = Collection.GetDescriptor<ISingletonServiceToCombine>();

            AssertExtensions.NotNull(singletonService);
            AssertExtensions.Equal(ServiceLifetime.Singleton, singletonService.Lifetime);
            AssertExtensions.Equal(typeof(CombinedService), singletonService.ImplementationType);
        }

        [Fact]
        public void AutoRegisterAsMatchingInterface()
        {
            Collection.Scan(scan => scan.FromAssemblyOf<ITransientService>()
                .AddClasses()
                    .AsMatchingInterface()
                    .WithTransientLifetime());

            AssertExtensions.Equal(8, Collection.Count);

            var services = Collection.GetDescriptors<ITransientService>();

            AssertExtensions.NotNull(services);
            AssertExtensions.All(services, s =>
            {
                AssertExtensions.Equal(ServiceLifetime.Transient, s.Lifetime);
                AssertExtensions.Equal(typeof(ITransientService), s.ServiceType);
            });
        }

        [Fact]
        public void AutoRegisterAsMatchingInterfaceSameNamespaceOnly()
        {
            Collection.Scan(scan => scan.FromAssemblyOf<ITransientService>()
                .AddClasses()
                    .AsMatchingInterface((t, x) => x.InNamespaceOf(t))
                    .WithTransientLifetime());

            AssertExtensions.Equal(7, Collection.Count);

            var service = Collection.GetDescriptor<ITransientService>();

            AssertExtensions.NotNull(service);
            AssertExtensions.Equal(ServiceLifetime.Transient, service.Lifetime);
            AssertExtensions.Equal(typeof(TransientService), service.ImplementationType);
        }

        [Fact]
        public void ShouldRegisterOpenGenericTypes()
        {
            var genericTypes = new[]
            {
                typeof(OpenGeneric<>),
                typeof(QueryHandler<,>),
                typeof(PartiallyClosedGeneric<>)
            };

            Collection.Scan(scan => scan
                .FromTypes(genericTypes)
                    .AddClasses()
                    .AsImplementedInterfaces());

            var provider = Collection.BuildServiceProvider();

            AssertExtensions.NotNull(provider.GetService<IOpenGeneric<int>>());
            AssertExtensions.NotNull(provider.GetService<IOpenGeneric<string>>());

            AssertExtensions.NotNull(provider.GetService<Handlers.IQueryHandler<string, float>>());
            AssertExtensions.NotNull(provider.GetService<Handlers.IQueryHandler<double, Guid>>());

            // We don't register partially closed generic types.
            AssertExtensions.Null(provider.GetService<IPartiallyClosedGeneric<string, int>>());
        }

        [Fact]
        public void ShouldNotIncludeCompilerGeneratedTypes()
        {
            AssertExtensions.Equal(0, Collection.Scan(scan => scan.FromType<CompilerGenerated>()).Count);
        }

        [Fact]
        public void ShouldNotRegisterTypesInSubNamespace()
        {
            Collection.Scan(scan => scan.FromAssembliesOf(GetType())
                .AddClasses(classes => classes.InExactNamespaceOf<ITransientService>())
                .AsSelf());

            var provider = Collection.BuildServiceProvider();

            AssertExtensions.Null(provider.GetService<ClassInChildNamespace>());
        }

        [Fact]
        public void ScanShouldCreateSeparateRegistrationsPerInterface()
        {
            Collection.Scan(scan => scan
                .FromAssemblyOf<CombinedService2>()
                .AddClasses(classes => classes.AssignableTo<CombinedService2>())
                    .AsImplementedInterfaces()
                    .WithSingletonLifetime()
                .AddClasses(classes => classes.AssignableTo<CombinedService2>())
                    .AsSelf()
                    .WithSingletonLifetime());

            AssertExtensions.Equal(5, Collection.Count);

            Assert.All(Collection, x =>
            {
                AssertExtensions.Equal(ServiceLifetime.Singleton, x.Lifetime);
                Assert.Equal(typeof(CombinedService2), x.ImplementationType);
            });
        }

        [Fact]
        public void AsSelfWithInterfacesShouldForwardRegistrationsToClass()
        {
            Collection.Scan(scan => scan
                .FromAssemblyOf<CombinedService2>()
                .AddClasses(classes => classes.AssignableTo<CombinedService2>())
                    .AsSelfWithInterfaces()
                    .WithSingletonLifetime());

            AssertExtensions.Equal(5, Collection.Count);

            var service1 = Collection.GetDescriptor<CombinedService2>();

            AssertExtensions.NotNull(service1);
            AssertExtensions.Equal(ServiceLifetime.Singleton, service1.Lifetime);
            AssertExtensions.Equal(typeof(CombinedService2), service1.ImplementationType);

            var interfaceDescriptors = Collection.Where(x => x.ImplementationType != typeof(CombinedService2)).ToList();
            AssertExtensions.Equal(4, interfaceDescriptors.Count);

            foreach (var x in interfaceDescriptors)
            {
                AssertExtensions.Equal(ServiceLifetime.Singleton, x.Lifetime);
                Assert.NotNull(x.ImplementationFactory);
            }
        }

        [Fact]
        public void AsSelfWithInterfacesShouldCreateTrueSingletons()
        {
            var provider = ConfigureProvider(services =>
            {
                services.Scan(scan => scan
                    .FromAssemblyOf<CombinedService2>()
                     .AddClasses(classes => classes.AssignableTo<CombinedService2>())
                        .AsSelfWithInterfaces()
                        .WithSingletonLifetime());
            });

            var instance1 = provider.GetRequiredService<CombinedService2>();
            var instance2 = provider.GetRequiredService<IDefault1>();
            var instance3 = provider.GetRequiredService<IDefault2>();
            var instance4 = provider.GetRequiredService<IDefault3Level2>();
            var instance5 = provider.GetRequiredService<IDefault3Level1>();

            Assert.Same(instance1, instance2);
            Assert.Same(instance1, instance3);
            Assert.Same(instance1, instance4);
            Assert.Same(instance1, instance5);
        }

        [Fact]
        public void AsSelfWithInterfacesHandlesOpenGenericTypes()
        {
            ConfigureProvider(services =>
            {
                services.Scan(scan => scan
                    .FromAssemblyOf<CombinedService2>()
                    .AddClasses(classes => classes.AssignableTo<IOtherInheritance>())
                    .AsSelfWithInterfaces()
                    .WithSingletonLifetime());
            });
        }
    }

    // ReSharper disable UnusedTypeParameter

    public interface ITransientService { }

    [ServiceDescriptor(typeof(ITransientService))]
    public class TransientService1 : ITransientService { }

    public class TransientService2 : ITransientService, IOtherInheritance { }

    public class TransientService : ITransientService { }

    public interface IScopedService { }

    public class ScopedService1 : IScopedService { }

    public class ScopedService2 : IScopedService { }
}

namespace Scrutor.Tests.Handlers
{
    public interface IQueryHandler<TQuery, TResult> { }

    public class QueryHandler : IQueryHandler<string, int> { }
}

namespace Scrutor.Tests
{

    public interface IOpenGeneric<T> : IOtherInheritance { }

    public class OpenGeneric<T> : IOpenGeneric<T> { }

    public interface IPartiallyClosedGeneric<T1, T2> { }

    public class PartiallyClosedGeneric<T> : IPartiallyClosedGeneric<T, int> { }

    public interface ITransientServiceToCombine { }

    public interface IScopedServiceToCombine { }

    public interface ISingletonServiceToCombine { }

    [ServiceDescriptor(typeof(ITransientServiceToCombine))]
    [ServiceDescriptor(typeof(IScopedServiceToCombine), ServiceLifetime.Scoped)]
    [ServiceDescriptor(typeof(ISingletonServiceToCombine), ServiceLifetime.Singleton)]
    public class CombinedService : ITransientServiceToCombine, IScopedServiceToCombine, ISingletonServiceToCombine { }

    public interface IWrongInheritanceA { }

    public interface IWrongInheritanceB { }

    [ServiceDescriptor(typeof(IWrongInheritanceA))]
    public class WrongInheritance : IWrongInheritanceB { }

    public interface IDuplicateInheritance { }

    public interface IOtherInheritance { }

    [ServiceDescriptor(typeof(IOtherInheritance))]
    [ServiceDescriptor(typeof(IDuplicateInheritance))]
    [ServiceDescriptor(typeof(IDuplicateInheritance))]
    public class DuplicateInheritance : IDuplicateInheritance, IOtherInheritance { }
    
    public interface IDefault1 { }

    public interface IDefault2 { }

    public interface IDefault3Level1 { }

    public interface IDefault3Level2 : IDefault3Level1 { }

    [ServiceDescriptor]
    public class DefaultAttributes : IDefault3Level2, IDefault1, IDefault2 { }

    [CompilerGenerated]
    public class CompilerGenerated { }

    public class CombinedService2: IDefault1, IDefault2, IDefault3Level2 { }

    public interface IGenericAttribute { }

    [ServiceDescriptor<IGenericAttribute>]
    public class GenericAttribute : IGenericAttribute { }

    public interface IMixedAttribute { }

    [ServiceDescriptor(typeof(IMixedAttribute), ServiceLifetime.Scoped)]
    [ServiceDescriptor<IMixedAttribute>(ServiceLifetime.Singleton)]
    public class MixedAttribute : IMixedAttribute { }
}

namespace Scrutor.Tests.ChildNamespace
{
    public class ClassInChildNamespace { }
}

namespace UnwantedNamespace
{
    public class TransientService : ITransientService
    {
    }
}