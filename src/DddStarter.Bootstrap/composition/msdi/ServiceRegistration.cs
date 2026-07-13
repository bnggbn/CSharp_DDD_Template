using System.IO;
using DddStarter.Application;
using DddStarter.Application.Contracts.Ports;
using DddStarter.Application.Workflows;
using DddStarter.Controller.Abstractions;
using DddStarter.Controller.Api;
using DddStarter.Controller.Cli;
using DddStarter.Controller.Console;
using DddStarter.Domain.Services;
using DddStarter.Infrastructure.Configuration;
using DddStarter.Infrastructure.Configuration.Abstractions;
using DddStarter.Infrastructure.Configuration.Rules;
using DddStarter.Infrastructure.Database.Core;
using DddStarter.Infrastructure.Logging;
using DddStarter.Infrastructure.Logging.Abstractions;
using DddStarter.Infrastructure.Logging.Rules;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DddStarter.Bootstrap.Composition;

/// <summary>
/// Provides the default Microsoft DI composition root for the DddStarter template.
/// </summary>
public static class ServiceRegistration
{
    /// <summary>
    /// Registers the template's application, infrastructure, workflow, MediatR, and controller services.
    /// </summary>
    /// <param name="services">The target service collection.</param>
    /// <param name="configuration">The application configuration source.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddDddStarter(this IServiceCollection services, IConfiguration configuration)
    {
        RegisterLogging(services);
        AppSettings appSettings = RegisterConfiguration(services, configuration);
        RegisterPersistence(services, appSettings);
        RegisterDomainServices(services);
        ApplicationUseCaseServiceCollectionRegistration.Register(services, typeof(ApplicationAssemblyMarker).Assembly);
        services.AddTransient<MonitoringWorkflow>();
        MediatRServiceCollectionRegistration.Register(services, CompositionAssemblies.All);
        RegisterControllers(services);
        services.AddSingleton<IAppController>(sp => sp.GetRequiredService<ConsoleController>());
        return services;
    }

    /// <summary>
    /// Registers logging abstractions, sanitization rules, and the application logger.
    /// </summary>
    /// <param name="services">The target service collection.</param>
    private static void RegisterLogging(IServiceCollection services)
    {
        services.AddSingleton<ILogSanitizationRule, AnsiEscapeRemovalRule>();
        services.AddSingleton<ILogSanitizationRule, SafeUnicodeRule>();
        services.AddSingleton<ILogSanitizationRule, LogMessageLengthRule>();
        services.AddSingleton<ILogSanitizer, DefaultLogSanitizer>();
        services.AddSingleton<IAppLogger, NLogAppLogger>();
    }

    /// <summary>
    /// Registers configuration sanitization services, binds application settings, and configures Data Protection.
    /// </summary>
    /// <param name="services">The target service collection.</param>
    /// <param name="configuration">The application configuration source.</param>
    /// <returns>The bound application settings.</returns>
    private static AppSettings RegisterConfiguration(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IConfigSanitizationRule, ControlAndC1ConfigRule>();
        services.AddSingleton<IConfigSanitizationRule, DirectionalConfigRule>();
        services.AddSingleton<IConfigSanitizer, ConfigSanitizer>();

        AppSettings appSettings = AppSettingsResolver.Bind(configuration);
        services.AddSingleton(appSettings);
        services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(AppSettingsResolver.GetKeyDirectoryPath(appSettings.DataProtection)))
            .SetApplicationName(appSettings.DataProtection.ApplicationName);

        return appSettings;
    }

    /// <summary>
    /// Registers pure domain services consumed by application handlers.
    /// </summary>
    /// <param name="services">The target service collection.</param>
    private static void RegisterDomainServices(IServiceCollection services)
    {
        services.AddSingleton<MonitoringExecutionService>();
    }

    /// <summary>
    /// Resolves protected connection strings and registers the default database context.
    /// </summary>
    /// <param name="services">The target service collection.</param>
    /// <param name="appSettings">The bound application settings.</param>
    private static void RegisterPersistence(IServiceCollection services, AppSettings appSettings)
    {
        IReadOnlyDictionary<string, string> connectionStrings = AppSettingsResolver.ResolveConnectionStrings(appSettings);
        services.AddSingleton(connectionStrings);

        string connectionString = connectionStrings[appSettings.Database.DefaultConnectionName];
        services.AddSingleton<IDbContextCore>(_ => new DapperDbContextCore(connectionString));
    }

    /// <summary>
    /// Registers controller adapters used as application entry points.
    /// </summary>
    /// <param name="services">The target service collection.</param>
    private static void RegisterControllers(IServiceCollection services)
    {
        services.AddSingleton<ConsoleController>();
        services.AddSingleton<CliController>();
        services.AddSingleton<ApiController>();
    }
}
