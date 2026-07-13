using System.Reflection;
using Autofac;
using DddStarter.Application.Behaviors;
using DddStarter.Application.Contracts.Ports;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DddStarter.Bootstrap.Composition;

/// <summary>
/// Registers MediatR services, handlers, validators, and pipeline behaviors into Autofac.
/// </summary>
internal static class MediatRAutofacRegistration
{
    /// <summary>
    /// Registers MediatR services into an Autofac container builder.
    /// </summary>
    /// <param name="builder">The target Autofac container builder.</param>
    /// <param name="assemblies">The assemblies to scan for MediatR components.</param>
    public static void Register(ContainerBuilder builder, Assembly[] assemblies)
    {
        builder.RegisterInstance(new MediatRServiceConfiguration()).SingleInstance();
        builder.RegisterInstance<ILoggerFactory>(NullLoggerFactory.Instance).SingleInstance();
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

        RegisterOpenGenericPipelineBehaviors(builder, assemblies);
    }

    /// <summary>
    /// Registers open generic pipeline behaviors into Autofac.
    /// </summary>
    /// <param name="builder">The target Autofac container builder.</param>
    /// <param name="assemblies">The assemblies to scan for pipeline behaviors.</param>
    private static void RegisterOpenGenericPipelineBehaviors(ContainerBuilder builder, IEnumerable<Assembly> assemblies)
    {
        foreach (Type behaviorType in PipelineBehaviorRegistration.FindOpenGenericBehaviorTypes(assemblies))
        {
            builder.RegisterGeneric(behaviorType)
                .As(typeof(IPipelineBehavior<,>))
                .InstancePerDependency();
        }
    }

    /// <summary>
    /// Adapts Autofac's component context to <see cref="IServiceProvider"/> for MediatR.
    /// </summary>
    private sealed class AutofacServiceProviderAdapter : IServiceProvider
    {
        private readonly IComponentContext _context;

        /// <summary>
        /// Initializes a new Autofac-backed service provider adapter.
        /// </summary>
        /// <param name="context">The Autofac component context.</param>
        public AutofacServiceProviderAdapter(IComponentContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Resolves an optional service from the underlying Autofac context.
        /// </summary>
        /// <param name="serviceType">The requested service type.</param>
        /// <returns>The resolved service, or <c>null</c> when no registration exists.</returns>
        public object? GetService(Type serviceType)
        {
            return _context.ResolveOptional(serviceType);
        }
    }
}
