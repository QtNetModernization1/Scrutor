using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Scrutor;

internal static class ServiceCollectionExtensions
{
    public static bool HasRegistration(this IServiceCollection services, Type serviceType)
    {
        foreach (var descriptor in services)
        {
            if (descriptor.ServiceType == serviceType)
            {
                return true;
            }
        }
        return false;
    }
}