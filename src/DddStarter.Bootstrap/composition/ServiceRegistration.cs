using DddStarter.Application.Contracts.Ports;
using DddStarter.Application.Contracts.UseCases;
using DddStarter.Application.Behaviors;
using DddStarter.Application.UseCases;
using DddStarter.Application.Workflows;
using DddStarter.Application;
using DddStarter.Controller.Abstractions;
using DddStarter.Controller.Api;
using DddStarter.Controller.Cli;
using DddStarter.Controller.Console;
using DddStarter.Controller;
using DddStarter.Domain;
using DddStarter.Infrastructure;
using DddStarter.Infrastructure.Configuration;
using DddStarter.Infrastructure.Configuration.Rules;
using DddStarter.Infrastructure.Database.Core;
using DddStarter.Infrastructure.Logging;
using DddStarter.Infrastructure.Logging.Rules;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace DddStarter.Bootstrap.Composition;

public static class ServiceRegistration
{
    public static IServiceCollection AddDddStarter(this IServiceCollection services, string connectionString = "")
    {
        services.AddSingleton<ILogSanitizationRule, AnsiEscapeRemovalRule>();
        services.AddSingleton<ILogSanitizationRule, SafeUnicodeRule>();
        services.AddSingleton<ILogSanitizationRule, LogMessageLengthRule>();
        services.AddSingleton<ILogSanitizer, DefaultLogSanitizer>();

        services.AddSingleton<IConfigSanitizationRule, ControlAndC1ConfigRule>();
        services.AddSingleton<IConfigSanitizationRule, DirectionalConfigRule>();
        services.AddSingleton<IConfigSanitizer, ConfigSanitizer>();

        services.AddSingleton<IAppLogger, NLogAppLogger>();
        services.AddSingleton<IDbContextCore>(_ => new DapperDbContextCore(connectionString));

        services.AddSingleton<IMonitoringExecutionUseCase, MonitoringExecutionUseCase>();
        services.AddTransient<MonitoringWorkflow>();
        RegisterMediatR(services);
        services.AddSingleton<ConsoleController>();
        services.AddSingleton<CliController>();
        services.AddSingleton<ApiController>();
        services.AddSingleton<IAppController>(sp => sp.GetRequiredService<ConsoleController>());
        return services;
    }

    private static void RegisterMediatR(IServiceCollection services)
    {
        Assembly[] assemblies =
        {
            typeof(BootstrapAssemblyMarker).Assembly,
            typeof(ApplicationAssemblyMarker).Assembly,
            typeof(InfrastructureAssemblyMarker).Assembly,
            typeof(DomainAssemblyMarker).Assembly,
            typeof(ControllerAssemblyMarker).Assembly
        };

        services.AddSingleton<IMediator>(sp => new Mediator(sp));
        services.AddSingleton<ISender>(sp => sp.GetRequiredService<IMediator>());
        services.AddSingleton<IPublisher>(sp => sp.GetRequiredService<IMediator>());

        RegisterClosedGenericImplementations(services, assemblies, typeof(IRequestHandler<,>));
        RegisterClosedGenericImplementations(services, assemblies, typeof(INotificationHandler<>));
        RegisterClosedGenericImplementations(services, assemblies, typeof(IRequestValidator<>));

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehavior<,>));
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
