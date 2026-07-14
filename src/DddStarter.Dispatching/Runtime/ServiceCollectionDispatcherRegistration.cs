using System.Reflection;
using DddStarter.Dispatching.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace DddStarter.Dispatching.Runtime;

public static class ServiceCollectionDispatcherRegistration
{
    public static void Register(IServiceCollection services, Assembly[] assemblies)
    {
        services.AddSingleton<IDispatcher, Dispatcher>();

        RegisterClosedGenericImplementations(services, assemblies, typeof(IRequestHandler<,>));
        RegisterOpenGenericPipelineBehaviors(services, assemblies);
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

    private static void RegisterOpenGenericPipelineBehaviors(IServiceCollection services, IEnumerable<Assembly> assemblies)
    {
        foreach (Type behaviorType in FindOpenGenericBehaviorTypes(assemblies))
        {
            services.AddTransient(typeof(IPipelineBehavior<,>), behaviorType);
        }
    }

    private static IEnumerable<Type> FindOpenGenericBehaviorTypes(IEnumerable<Assembly> assemblies)
    {
        return assemblies
            .SelectMany(static assembly => assembly.GetTypes())
            .Where(static type => type is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: true })
            .Where(static type => type.GetInterfaces()
                .Any(static iface => iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>)))
            .Distinct();
    }
}