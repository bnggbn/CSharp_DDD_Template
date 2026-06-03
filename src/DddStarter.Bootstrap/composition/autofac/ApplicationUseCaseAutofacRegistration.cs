using System.Reflection;
using Autofac;

namespace DddStarter.Bootstrap.Composition;

/// <summary>
/// Registers application use-case service contracts into Autofac.
/// </summary>
internal static class ApplicationUseCaseAutofacRegistration
{
    /// <summary>
    /// Registers application use-case contracts into an Autofac container builder.
    /// </summary>
    /// <param name="builder">The target Autofac container builder.</param>
    /// <param name="applicationAssembly">The application assembly containing implementations.</param>
    public static void Register(ContainerBuilder builder, Assembly applicationAssembly)
    {
        foreach ((Type serviceType, Type implementationType) in ApplicationContractRegistration.FindUseCaseRegistrations(applicationAssembly))
        {
            builder.RegisterType(implementationType)
                .As(serviceType)
                .InstancePerDependency();
        }
    }
}
