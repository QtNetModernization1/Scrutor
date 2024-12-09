using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Collections.Generic;

namespace Scrutor;

public abstract class RegistrationStrategy
{
    /// <summary>
    /// Skips registrations for services that already exists.
    /// </summary>
    public static readonly RegistrationStrategy Skip = new SkipRegistrationStrategy();

    /// <summary>
    /// Appends a new registration for existing services.
    /// </summary>
    public static readonly RegistrationStrategy Append = new AppendRegistrationStrategy();

    /// <summary>
    /// Throws when trying to register an existing service.
    /// </summary>
    public static readonly RegistrationStrategy Throw = new ThrowRegistrationStrategy();

    /// <summary>
/// Replaces existing service registrations using <see cref="ReplacementBehavior.Default"/>.
    /// </summary>
    public static RegistrationStrategy Replace()
    {
        return Replace(ReplacementBehavior.Default);
    }

    /// <summary>
    /// Replaces existing service registrations based on the specified <see cref="ReplacementBehavior"/>.
    /// </summary>
    /// <param name="behavior">The behavior to use when replacing services.</param>
    public static RegistrationStrategy Replace(ReplacementBehavior behavior)
    {
        return new ReplaceRegistrationStrategy(behavior);
    }

    /// <summary>
    /// Applies the the <see cref="ServiceDescriptor"/> to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="descriptor">The descriptor to apply.</param>
    public abstract void Apply(IEnumerable<ServiceDescriptor> services, ServiceDescriptor descriptor);

    private sealed class SkipRegistrationStrategy : RegistrationStrategy
    {
        public override void Apply(IEnumerable<ServiceDescriptor> services, ServiceDescriptor descriptor) =>
            (services as IServiceCollection)?.TryAdd(descriptor);
    }

    private sealed class AppendRegistrationStrategy : RegistrationStrategy
    {
        public override void Apply(IEnumerable<ServiceDescriptor> services, ServiceDescriptor descriptor) => ((IServiceCollection)services).Add(descriptor);
    }

    private sealed class ThrowRegistrationStrategy : RegistrationStrategy
    {
        public override void Apply(IEnumerable<ServiceDescriptor> services, ServiceDescriptor descriptor)
        {
            var serviceCollection = services as IServiceCollection;
            if (serviceCollection == null)
            {
                throw new System.ArgumentException("Services must be of type IServiceCollection", nameof(services));
            }

            if (serviceCollection.HasRegistration(descriptor.ServiceType))
            {
                throw new DuplicateTypeRegistrationException(descriptor.ServiceType);
            }

            serviceCollection.Add(descriptor);
        }
    }

    private sealed class ReplaceRegistrationStrategy : RegistrationStrategy
    {
        public ReplaceRegistrationStrategy(ReplacementBehavior behavior)
        {
            Behavior = behavior;
        }

        private ReplacementBehavior Behavior { get; }

        public override void Apply(IEnumerable<ServiceDescriptor> services, ServiceDescriptor descriptor)
        {
            if (services is not IServiceCollection serviceCollection)
            {
                throw new System.ArgumentException("Services must be of type IServiceCollection", nameof(services));
            }

            var behavior = Behavior;

            if (behavior == ReplacementBehavior.Default)
            {
                behavior = ReplacementBehavior.ServiceType;
            }

            if (behavior.HasFlag(ReplacementBehavior.ServiceType))
            {
                for (var i = serviceCollection.Count - 1; i >= 0; i--)
                {
                    if (serviceCollection[i].ServiceType == descriptor.ServiceType)
                    {
                        serviceCollection.RemoveAt(i);
                    }
                }
            }

            if (behavior.HasFlag(ReplacementBehavior.ImplementationType))
            {
                for (var i = serviceCollection.Count - 1; i >= 0; i--)
                {
                    if (serviceCollection[i].ImplementationType == descriptor.ImplementationType)
                    {
                        serviceCollection.RemoveAt(i);
                    }
                }
            }

            serviceCollection.Add(descriptor);
        }
    }
}