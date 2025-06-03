using System;
using Microsoft.Extensions.DependencyInjection;

namespace Scrutor.Tests;

public class ServiceProviderTestBase
{
    protected static ServiceProvider ConfigureProvider(Action<IServiceCollection> configure)
    {
        var services = new ServiceCollection();

        configure(services);

        return services.BuildServiceProvider();
    }
}