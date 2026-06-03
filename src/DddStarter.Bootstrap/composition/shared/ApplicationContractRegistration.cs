using System.Reflection;

namespace DddStarter.Bootstrap.Composition;

/// <summary>
/// Finds application use-case contract to implementation mappings inside the application assembly.
/// </summary>
internal static class ApplicationContractRegistration
{
    private const string UseCaseContractNamespace = "DddStarter.Application.Contracts.UseCases";

    /// <summary>
    /// Scans the application assembly for concrete types implementing contracts under <c>Contracts.UseCases</c>.
    /// </summary>
    /// <param name="applicationAssembly">The application assembly to scan.</param>
    /// <returns>A sequence of service and implementation type pairs.</returns>
    public static IEnumerable<(Type ServiceType, Type ImplementationType)> FindUseCaseRegistrations(Assembly applicationAssembly)
    {
        Type[] applicationTypes = applicationAssembly.GetTypes();

        IEnumerable<Type> implementations = applicationTypes
            .Where(static type => type is { IsClass: true, IsAbstract: false });

        foreach (Type implementation in implementations)
        {
            IEnumerable<Type> serviceTypes = implementation.GetInterfaces()
                .Where(static iface =>
                    string.Equals(iface.Namespace, UseCaseContractNamespace, StringComparison.Ordinal) &&
                    iface.Name.EndsWith("UseCase", StringComparison.Ordinal));

            foreach (Type serviceType in serviceTypes)
            {
                yield return (serviceType, implementation);
            }
        }
    }
}
