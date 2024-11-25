using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Scrutor;

internal static class ServiceCollectionExtensions
{
    public static bool HasRegistration(this Microsoft.Extensions.DependencyInjection.IServiceCollection services, Type serviceType)
    {
        return services.Any(x => x.ServiceType == serviceType);
    }
}