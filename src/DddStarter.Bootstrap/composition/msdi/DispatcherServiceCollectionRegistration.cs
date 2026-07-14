using System.Reflection;
using DddStarter.Application.Contracts.Ports;
using DddStarter.Dispatching.Runtime;
using Microsoft.Extensions.DependencyInjection;

namespace DddStarter.Bootstrap.Composition;

/// <summary>
/// Registers dispatcher services, validators, and pipeline behaviors into Microsoft DI.
/// </summary>
internal static class DispatcherServiceCollectionRegistration
{
    /// <summary>
    /// Registers dispatcher services into a Microsoft DI service collection.
    /// </summary>
    /// <param name="services">The target service collection.</param>
    /// <param name="assemblies">The assemblies to scan for dispatcher components.</param>
    public static void Register(IServiceCollection services, Assembly[] assemblies)
    {
        ServiceCollectionDispatcherRegistration.Register(services, assemblies);
        RegisterClosedGenericImplementations(services, assemblies, typeof(IRequestValidator<>));
    }

    private static void RegisterClosedGenericImplementations(IServiceCollection services, Assembly[] assemblies, Type openGeneric)
    {
        IEnumerable<Type> implementations = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type is { IsAbstract: false, IsInterface: false })
            .Where(type => type.GetInterfaces().Any(iface => iface.IsGenericType && iface.GetGenericTypeDefinition() == openGeneric));

        foreach (Type implementation in implementations)
        {
            IEnumerable<Type> interfaces = implementation.GetInterfaces()
                .Where(iface => iface.IsGenericType && iface.GetGenericTypeDefinition() == openGeneric);

            foreach (Type serviceType in interfaces)
            {
                services.AddTransient(serviceType, implementation);
            }
        }
    }
}