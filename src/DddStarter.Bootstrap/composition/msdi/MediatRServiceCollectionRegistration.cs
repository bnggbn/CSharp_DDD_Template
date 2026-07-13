using System.Reflection;
using DddStarter.Application.Behaviors;
using DddStarter.Application.Contracts.Ports;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DddStarter.Bootstrap.Composition;

/// <summary>
/// Registers MediatR services, handlers, validators, and pipeline behaviors into Microsoft DI.
/// </summary>
internal static class MediatRServiceCollectionRegistration
{
    /// <summary>
    /// Registers MediatR services into a Microsoft DI service collection.
    /// </summary>
    /// <param name="services">The target service collection.</param>
    /// <param name="assemblies">The assemblies to scan for MediatR components.</param>
    public static void Register(IServiceCollection services, Assembly[] assemblies)
    {
        services.AddSingleton(new MediatRServiceConfiguration());
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton<IMediator>(sp => new Mediator(sp));
        services.AddSingleton<ISender>(sp => sp.GetRequiredService<IMediator>());
        services.AddSingleton<IPublisher>(sp => sp.GetRequiredService<IMediator>());

        RegisterClosedGenericImplementations(services, assemblies, typeof(IRequestHandler<,>));
        RegisterClosedGenericImplementations(services, assemblies, typeof(INotificationHandler<>));
        RegisterClosedGenericImplementations(services, assemblies, typeof(IRequestValidator<>));
        RegisterOpenGenericPipelineBehaviors(services, assemblies);
    }

    /// <summary>
    /// Registers closed generic implementations such as handlers and validators into Microsoft DI.
    /// </summary>
    /// <param name="services">The target service collection.</param>
    /// <param name="assemblies">The assemblies to scan.</param>
    /// <param name="openGeneric">The open generic service contract to match.</param>
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

    /// <summary>
    /// Registers open generic pipeline behaviors into Microsoft DI.
    /// </summary>
    /// <param name="services">The target service collection.</param>
    /// <param name="assemblies">The assemblies to scan for pipeline behaviors.</param>
    private static void RegisterOpenGenericPipelineBehaviors(IServiceCollection services, IEnumerable<Assembly> assemblies)
    {
        foreach (Type behaviorType in PipelineBehaviorRegistration.FindOpenGenericBehaviorTypes(assemblies))
        {
            services.AddTransient(typeof(IPipelineBehavior<,>), behaviorType);
        }
    }
}
