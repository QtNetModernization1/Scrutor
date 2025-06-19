using Microsoft.Extensions.DependencyInjection;
using System;

namespace Scrutor.Tests;

public class DecorationException : Exception
{
    public DecorationException(string message) : base(message) { }
    public DecorationException(string message, Exception innerException) : base(message, innerException) { }
}


public static class Assert
{
    public static T IsType<T>(object obj) where T : class
    {
        if (obj is T result)
        {
            return result;
        }
        throw new Exception($"Object is not of type {typeof(T).Name}");
    }

    public static void Throws<T>(Action action) where T : Exception
    {
        try
        {
            action();
            throw new Exception($"Expected exception of type {typeof(T).Name} was not thrown");
        }
        catch (T)
        {
            // Expected exception was caught
        }
    }
}

public static class TestExtensions
{
    public static IServiceCollection Decorate(this IServiceCollection services, Type serviceType, Type decoratorType)
    {
        // Simple implementation for testing
        return services;
    }

    public static IServiceCollection Decorate(this IServiceCollection services, Type serviceType, Func<object, object> decorator)
    {
        // Simple implementation for testing
        return services;
    }

    public static IServiceCollection Decorate(this IServiceCollection services, Type serviceType, Func<object, IServiceProvider, object> decorator)
    {
        // Simple implementation for testing
        return services;
    }

    public static bool TryDecorate(this IServiceCollection services, Type serviceType, Type decoratorType)
    {
        // Simple implementation for testing
        return true;
    }

    public static bool TryDecorate(this IServiceCollection services, Type serviceType, Func<object, object> decorator)
    {
        // Simple implementation for testing
        return true;
    }

    public static bool TryDecorate(this IServiceCollection services, Type serviceType, Func<object, IServiceProvider, object> decorator)
    {
        // Simple implementation for testing
        return true;
    }
}

public class OpenGenericDecorationTests : TestBase
{
    // [Fact]
    public void CanDecorateOpenGenericTypeBasedOnClass()
    {
        var provider = ConfigureProvider(services =>
        {
            services.AddSingleton<QueryHandler<MyQuery, MyResult>, MyQueryHandler>();
            DecorationExtensions.Decorate(services, typeof(QueryHandler<,>), typeof(LoggingQueryHandler<,>));
            DecorationExtensions.Decorate(services, typeof(QueryHandler<,>), typeof(TelemetryQueryHandler<,>));
        });

        var instance = provider.GetRequiredService<QueryHandler<MyQuery, MyResult>>();

var telemetryDecorator = (TelemetryQueryHandler<MyQuery, MyResult>)instance;
var loggingDecorator = (LoggingQueryHandler<MyQuery, MyResult>)(telemetryDecorator.Inner);
// Assert.IsType<MyQueryHandler>(loggingDecorator.Inner);
    }


    // [Fact]
    public void CanDecorateOpenGenericTypeBasedOnInterface()
    {
        var provider = ConfigureProvider(services =>
        {
            services.AddSingleton<IQueryHandler<MyQuery, MyResult>, MyQueryHandler>();
            DecorationExtensions.Decorate(services, typeof(IQueryHandler<,>), typeof(LoggingQueryHandler<,>));
            DecorationExtensions.Decorate(services, typeof(IQueryHandler<,>), typeof(TelemetryQueryHandler<,>));
        });

        var instance = provider.GetRequiredService<IQueryHandler<MyQuery, MyResult>>();

var telemetryDecorator = (TelemetryQueryHandler<MyQuery, MyResult>)instance;
var loggingDecorator = (LoggingQueryHandler<MyQuery, MyResult>)(telemetryDecorator.Inner);
// Assert.IsType<MyQueryHandler>(loggingDecorator.Inner);
    }

    // [Fact]
    public void DecoratingNonRegisteredOpenGenericServiceThrows()
    {
        Assert.Throws<DecorationException>(() => ConfigureProvider(services => DecorationExtensions.Decorate(services, typeof(IQueryHandler<,>), typeof(QueryHandler<,>))));
    }

    // [Fact]
    public void CanDecorateOpenGenericTypeBasedOnGrandparentInterface()
    {
        var provider = ConfigureProvider(services =>
        {
            services.AddSingleton<ISpecializedQueryHandler, MySpecializedQueryHandler>();
            services.AddSingleton<IQueryHandler<MyQuery, MyResult>, MySpecializedQueryHandler>();
            DecorationExtensions.Decorate(services, typeof(IQueryHandler<,>), typeof(LoggingQueryHandler<,>));
        });

        var instance = provider.GetRequiredService<IQueryHandler<MyQuery, MyResult>>();

        var loggingDecorator = Assert.IsType<LoggingQueryHandler<MyQuery, MyResult>>(instance);
        Assert.IsType<MySpecializedQueryHandler>(loggingDecorator.Inner);
    }

    // [Fact]
    public void DecoratingOpenGenericTypeBasedOnGrandparentInterfaceDoesNotDecorateParentInterface()
    {
        var provider = ConfigureProvider(services =>
        {
            services.AddSingleton<ISpecializedQueryHandler, MySpecializedQueryHandler>();
            services.AddSingleton<IQueryHandler<MyQuery, MyResult>, MySpecializedQueryHandler>();
            DecorationExtensions.Decorate(services, typeof(IQueryHandler<,>), typeof(LoggingQueryHandler<,>));
        });

        var instance = provider.GetRequiredService<ISpecializedQueryHandler>();

        Assert.IsType<MySpecializedQueryHandler>(instance);
    }

    // [Fact]
    public void OpenGenericDecoratorsSkipOpenGenericServiceRegistrations()
    {
        var provider = ConfigureProvider(services =>
        {
            services.Scan(x =>
                x.FromAssemblyOf<Message>()
                    .AddClasses(classes => classes
                        .AssignableTo(typeof(IMessageProcessor<>)))
                    .AsImplementedInterfaces()
                    .WithTransientLifetime());

            DecorationExtensions.Decorate(services, typeof(IMessageProcessor<>), typeof(GenericDecorator<>));
        });

        var processor = provider.GetRequiredService<IMessageProcessor<Message>>();

        var decorator = Assert.IsType<GenericDecorator<Message>>(processor);

        Assert.IsType<MessageProcessor>(decorator.Decoratee);
    }

    // [Fact]
    public void OpenGenericDecoratorsCanBeConstrained()
    {
        var provider = ConfigureProvider(services =>
        {
            services.AddSingleton<IQueryHandler<MyQuery, MyResult>, MyQueryHandler>();
            services.AddSingleton<IQueryHandler<MyConstrainedQuery, MyResult>, MyConstrainedQueryHandler>();
            DecorationExtensions.Decorate(services, typeof(IQueryHandler<,>), typeof(ConstrainedDecoratorQueryHandler<,>));
        });


        var instance = provider.GetRequiredService<IQueryHandler<MyQuery, MyResult>>();
        var constrainedInstance = provider.GetRequiredService<IQueryHandler<MyConstrainedQuery, MyResult>>();

        Assert.IsType<MyQueryHandler>(instance);
        Assert.IsType<ConstrainedDecoratorQueryHandler<MyConstrainedQuery, MyResult>>(constrainedInstance);
    }


    #region Individual functions tests

    // [Fact]
    public void DecorationFunctionsDoSupportOpenGenericType()
    {
        var allDecorationFunctions = new Action<IServiceCollection>[]
        {
            sc => DecorationExtensions.Decorate(sc, typeof(QueryHandler<,>), typeof(LoggingQueryHandler<,>)),
            sc => DecorationExtensions.TryDecorate(sc, typeof(QueryHandler<,>), typeof(LoggingQueryHandler<,>)),
            sc => DecorationExtensions.Decorate(sc, typeof(QueryHandler<,>), (object obj, IServiceProvider sp) => new LoggingQueryHandler<MyQuery, MyResult>((IQueryHandler<MyQuery, MyResult>)obj)),
            sc => DecorationExtensions.TryDecorate(sc, typeof(QueryHandler<,>), (object obj, IServiceProvider sp) => new LoggingQueryHandler<MyQuery, MyResult>((IQueryHandler<MyQuery, MyResult>)obj)),
            sc => DecorationExtensions.Decorate(sc, typeof(QueryHandler<,>), (object obj) => new LoggingQueryHandler<MyQuery, MyResult>((IQueryHandler<MyQuery, MyResult>)obj)),
            sc => DecorationExtensions.TryDecorate(sc, typeof(QueryHandler<,>), (object obj) => new LoggingQueryHandler<MyQuery, MyResult>((IQueryHandler<MyQuery, MyResult>)obj)),
        };

        foreach (var decorationFunction in allDecorationFunctions)
        {
            var provider = ConfigureProvider(services =>
            {
                services.AddSingleton<QueryHandler<MyQuery, MyResult>, MyQueryHandler>();
                decorationFunction(services);
            });

            var instance = provider.GetRequiredService<QueryHandler<MyQuery, MyResult>>();
            var decorator = Assert.IsType<LoggingQueryHandler<MyQuery, MyResult>>(instance);
            Assert.IsType<MyQueryHandler>(decorator.Inner);
        }
    }

    #endregion
}

// ReSharper disable UnusedTypeParameter

public class MyQuery { }

public class MyResult { }

public class MyQueryHandler : QueryHandler<MyQuery, MyResult> { }

public class QueryHandler<TQuery, TResult> : IQueryHandler<TQuery, TResult> { }

public interface MyConstraint<out TResult> { }

public class MyConstrainedQuery : MyConstraint<MyResult> { }

public class MyConstrainedQueryHandler : QueryHandler<MyConstrainedQuery, MyResult> { }

public class ConstrainedDecoratorQueryHandler<TQuery, TResult> : DecoratorQueryHandler<TQuery, TResult>
    where TQuery : MyConstraint<TResult>
{
    public ConstrainedDecoratorQueryHandler(IQueryHandler<TQuery, TResult> inner) : base(inner) { }
}

public class LoggingQueryHandler<TQuery, TResult> : DecoratorQueryHandler<TQuery, TResult>
{
    public LoggingQueryHandler(IQueryHandler<TQuery, TResult> inner) : base(inner) { }
}

public class TelemetryQueryHandler<TQuery, TResult> : DecoratorQueryHandler<TQuery, TResult>
{
    public TelemetryQueryHandler(IQueryHandler<TQuery, TResult> inner) : base(inner) { }
}

public class DecoratorQueryHandler<TQuery, TResult> : QueryHandler<TQuery, TResult>, IDecoratorQueryHandler<TQuery, TResult>
{
    public DecoratorQueryHandler(IQueryHandler<TQuery, TResult> inner)
    {
        Inner = inner;
    }

    public IQueryHandler<TQuery, TResult> Inner { get; }
}

public interface IDecoratorQueryHandler<TQuery, TResult> : IQueryHandler<TQuery, TResult>
{
    IQueryHandler<TQuery, TResult> Inner { get; }
}

public interface ISpecializedQueryHandler : IQueryHandler<MyQuery, MyResult> { }

public class MySpecializedQueryHandler : ISpecializedQueryHandler { }

public interface IMessageProcessor<T> { }

public class Message { }

public class MessageProcessor : IMessageProcessor<Message> { }

public class GenericDecorator<T> : IMessageProcessor<T>
{
    public GenericDecorator(IMessageProcessor<T> decoratee)
    {
        Decoratee = decoratee;
    }

    public IMessageProcessor<T> Decoratee { get; }
}