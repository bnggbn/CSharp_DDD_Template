using System.Reflection;
using Autofac;
using Amazon;
using Amazon.SecretsManager;
using DddStarter.Application.Contracts.Ports;
using DddStarter.Dispatching.Runtime;
using DddStarter.Infrastructure.Configuration;
using DddStarter.Infrastructure.Database.Core;
using DddStarter.Infrastructure.Logging;
using DddStarter.Infrastructure.Logging.Abstractions;
using Microsoft.Extensions.Configuration;

namespace DddStarter.Bootstrap.Composition;

/// <summary>
/// Provides the default Autofac composition root for the DddStarter template.
/// </summary>
public static class ContainerFactory
{
    /// <summary>
    /// Builds an Autofac container using the template's application, infrastructure, and dispatching registrations.
    /// </summary>
    /// <param name="configuration">The application configuration source.</param>
    /// <returns>A fully configured Autofac container.</returns>
    public static IContainer Build(IConfiguration configuration)
    {
        ContainerBuilder builder = new();
        Assembly[] assemblies = CompositionAssemblies.All;
        AppSettings appSettings = AppSettingsResolver.Bind(configuration);

        builder.RegisterAssemblyTypes(assemblies)
            .AsImplementedInterfaces()
            .AsSelf()
            .InstancePerDependency();

        builder.RegisterInstance(appSettings).SingleInstance();
        builder.RegisterType<DefaultLogSanitizer>().As<ILogSanitizer>().SingleInstance();
        builder.RegisterType<NLogAppLogger>().As<IAppLogger>().SingleInstance();
        RegisterConnectionStringServices(builder, appSettings);
        ApplicationUseCaseAutofacRegistration.Register(builder, typeof(DddStarter.Application.ApplicationAssemblyMarker).Assembly);
        builder.Register(ctx =>
            {
                IConnectionStringProvider connectionStringProvider = ctx.Resolve<IConnectionStringProvider>();
                string connectionString = connectionStringProvider.GetRequiredConnectionString(appSettings.Database.DefaultConnectionName);
                return new DapperDbContextCore(connectionString);
            })
            .As<IDbContextCore>()
            .SingleInstance();
        DispatcherAutofacRegistration.Register(builder, assemblies);

        return builder.Build();
    }

    private static void RegisterConnectionStringServices(ContainerBuilder builder, AppSettings appSettings)
    {
        switch (appSettings.ConnectionStringProvider.Kind)
        {
            case ConnectionStringProviderKinds.DataProtection:
                builder.RegisterType<DataProtectionConnectionStringProvider>().As<IConnectionStringProvider>().SingleInstance();
                builder.RegisterType<ConnectionStringSecretProtector>().As<IConnectionStringSecretProtector>().SingleInstance();
                break;
            case ConnectionStringProviderKinds.Environment:
                builder.RegisterType<EnvironmentConnectionStringProvider>().As<IConnectionStringProvider>().SingleInstance();
                builder.Register(_ => new UnsupportedConnectionStringSecretProtector(appSettings.ConnectionStringProvider.Kind))
                    .As<IConnectionStringSecretProtector>()
                    .SingleInstance();
                break;
            case ConnectionStringProviderKinds.AwsSecretsManager:
                builder.Register(_ => new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(appSettings.AwsSecretsManager.RegionSystemName)))
                    .As<IAmazonSecretsManager>()
                    .SingleInstance();
                builder.RegisterType<AwsSecretsManagerConnectionStringProvider>().As<IConnectionStringProvider>().SingleInstance();
                builder.Register(_ => new UnsupportedConnectionStringSecretProtector(appSettings.ConnectionStringProvider.Kind))
                    .As<IConnectionStringSecretProtector>()
                    .SingleInstance();
                break;
            default:
                throw new InvalidOperationException($"Unsupported AppSettings:ConnectionStringProvider:Kind '{appSettings.ConnectionStringProvider.Kind}'.");
        }
    }
}
