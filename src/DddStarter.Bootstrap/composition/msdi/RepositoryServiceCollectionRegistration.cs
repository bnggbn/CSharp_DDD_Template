using System.Reflection;
using DddStarter.Infrastructure.Database.Abstractions.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace DddStarter.Bootstrap.Composition;

/// <summary>
/// Registers infrastructure repository contracts into Microsoft DI.
/// </summary>
internal static class RepositoryServiceCollectionRegistration
{
    /// <summary>
    /// Registers repository implementations from the infrastructure assembly into a Microsoft DI service collection.
    /// </summary>
    /// <param name="services">The target service collection.</param>
    /// <param name="infrastructureAssembly">The infrastructure assembly containing repository implementations.</param>
    public static void Register(IServiceCollection services, Assembly infrastructureAssembly)
    {
        IEnumerable<Type> implementationTypes = infrastructureAssembly.GetTypes()
            .Where(type => type is { IsAbstract: false, IsInterface: false })
            .Where(type => type.GetInterfaces().Any(IsRepositoryContract));

        foreach (Type implementationType in implementationTypes)
        {
            IEnumerable<Type> serviceTypes = implementationType.GetInterfaces().Where(IsRepositoryContract);

            foreach (Type serviceType in serviceTypes)
            {
                services.AddTransient(serviceType, implementationType);
            }
        }
    }

    private static bool IsRepositoryContract(Type serviceType)
    {
        if (!serviceType.IsInterface)
        {
            return false;
        }

        if (serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(IRepository<>))
        {
            return true;
        }

        return serviceType.GetInterfaces().Any(parent => parent.IsGenericType && parent.GetGenericTypeDefinition() == typeof(IRepository<>));
    }
}