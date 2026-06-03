using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace DddStarter.Bootstrap.Composition;

/// <summary>
/// Registers application use-case service contracts into Microsoft DI.
/// </summary>
internal static class ApplicationUseCaseServiceCollectionRegistration
{
    /// <summary>
    /// Registers application use-case contracts into a Microsoft DI service collection.
    /// </summary>
    /// <param name="services">The target service collection.</param>
    /// <param name="applicationAssembly">The application assembly containing implementations.</param>
    public static void Register(IServiceCollection services, Assembly applicationAssembly)
    {
        foreach ((Type serviceType, Type implementationType) in ApplicationContractRegistration.FindUseCaseRegistrations(applicationAssembly))
        {
            services.AddTransient(serviceType, implementationType);
        }
    }
}
