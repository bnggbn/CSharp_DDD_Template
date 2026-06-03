using System.Reflection;
using Autofac;
using DddStarter.Application.Contracts.Ports;
using DddStarter.Infrastructure.Configuration;
using DddStarter.Infrastructure.Database.Core;
using DddStarter.Infrastructure.Logging;
using Microsoft.Extensions.Configuration;

namespace DddStarter.Bootstrap.Composition;

/// <summary>
/// Provides the default Autofac composition root for the DddStarter template.
/// </summary>
public static class ContainerFactory
{
    /// <summary>
    /// Builds an Autofac container using the template's application, infrastructure, and MediatR registrations.
    /// </summary>
    /// <param name="configuration">The application configuration source.</param>
    /// <returns>A fully configured Autofac container.</returns>
    public static IContainer Build(IConfiguration configuration)
    {
        ContainerBuilder builder = new();
        Assembly[] assemblies = CompositionAssemblies.All;
        AppSettings appSettings = AppSettingsResolver.Bind(configuration);
        IReadOnlyDictionary<string, string> connectionStrings = AppSettingsResolver.ResolveConnectionStrings(appSettings);
        string connectionString = connectionStrings[appSettings.Database.DefaultConnectionName];

        builder.RegisterAssemblyTypes(assemblies)
            .AsImplementedInterfaces()
            .AsSelf()
            .InstancePerDependency();

        builder.RegisterInstance(appSettings).SingleInstance();
        builder.RegisterInstance(connectionStrings).SingleInstance();
        builder.RegisterType<DefaultLogSanitizer>().As<ILogSanitizer>().SingleInstance();
        builder.RegisterType<NLogAppLogger>().As<IAppLogger>().SingleInstance();
        ApplicationUseCaseAutofacRegistration.Register(builder, typeof(DddStarter.Application.ApplicationAssemblyMarker).Assembly);
        builder.Register(_ => new DapperDbContextCore(connectionString)).As<IDbContextCore>().SingleInstance();
        MediatRAutofacRegistration.Register(builder, assemblies);

        return builder.Build();
    }
}
