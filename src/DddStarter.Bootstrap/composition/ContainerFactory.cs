using System.Reflection;
using Autofac;
using DddStarter.Application;
using DddStarter.Application.Contracts.Ports;
using DddStarter.Application.Contracts.UseCases;
using DddStarter.Application.UseCases.Pipeline;
using DddStarter.Application.UseCases;
using DddStarter.Controller;
using DddStarter.Domain;
using DddStarter.Infrastructure;
using DddStarter.Infrastructure.Database.Core;
using DddStarter.Infrastructure.Logging;
using MediatR;

namespace DddStarter.Bootstrap.Composition;

public static class ContainerFactory
{
    public static IContainer Build(string connectionString)
    {
        ContainerBuilder builder = new();
        Assembly[] assemblies =
        {
            typeof(BootstrapAssemblyMarker).Assembly,
            typeof(DomainAssemblyMarker).Assembly,
            typeof(ApplicationAssemblyMarker).Assembly,
            typeof(InfrastructureAssemblyMarker).Assembly,
            typeof(ControllerAssemblyMarker).Assembly
        };

        builder.RegisterAssemblyTypes(assemblies)
            .AsImplementedInterfaces()
            .AsSelf()
            .InstancePerDependency();

        builder.RegisterType<DefaultLogSanitizer>().As<ILogSanitizer>().SingleInstance();
        builder.RegisterType<NLogAppLogger>().As<IAppLogger>().SingleInstance();
        builder.RegisterType<MonitoringExecutionUseCase>().As<IMonitoringExecutionUseCase>().InstancePerDependency();
        builder.Register(_ => new DapperDbContextCore(connectionString)).As<IDbContextCore>().SingleInstance();

        RegisterMediatR(builder, assemblies);

        return builder.Build();
    }

    private static void RegisterMediatR(ContainerBuilder builder, Assembly[] assemblies)
    {
        builder.Register(ctx =>
            {
                IComponentContext componentContext = ctx.Resolve<IComponentContext>();
                return new Mediator(new AutofacServiceProviderAdapter(componentContext));
            })
            .As<IMediator>()
            .InstancePerLifetimeScope();

        builder.Register(context => context.Resolve<IMediator>()).As<ISender>().InstancePerLifetimeScope();
        builder.Register(context => context.Resolve<IMediator>()).As<IPublisher>().InstancePerLifetimeScope();

        builder.RegisterAssemblyTypes(assemblies)
            .AsClosedTypesOf(typeof(IRequestHandler<,>))
            .InstancePerDependency();

        builder.RegisterAssemblyTypes(assemblies)
            .AsClosedTypesOf(typeof(INotificationHandler<>))
            .InstancePerDependency();

        builder.RegisterAssemblyTypes(assemblies)
            .AsClosedTypesOf(typeof(IRequestValidator<>))
            .AsImplementedInterfaces()
            .InstancePerDependency();

        builder.RegisterGeneric(typeof(ValidationBehavior<,>))
            .As(typeof(IPipelineBehavior<,>))
            .InstancePerDependency();

        builder.RegisterGeneric(typeof(UnhandledExceptionBehavior<,>))
            .As(typeof(IPipelineBehavior<,>))
            .InstancePerDependency();
    }

    private sealed class AutofacServiceProviderAdapter : IServiceProvider
    {
        private readonly IComponentContext _context;

        public AutofacServiceProviderAdapter(IComponentContext context)
        {
            _context = context;
        }

        public object? GetService(Type serviceType)
        {
            return _context.ResolveOptional(serviceType);
        }
    }
}
