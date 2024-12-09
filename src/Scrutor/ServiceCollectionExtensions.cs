using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Scrutor
{
    internal static class ServiceCollectionExtensions
    {
        public static bool HasRegistration(this IServiceCollection services, Type serviceType)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));

            return services.Any(x => x.ServiceType == serviceType);
        }
    }
}