using System.Reflection;
using Autofac;
using DddStarter.Dispatching.Contracts;

namespace DddStarter.Dispatching.Runtime;

public static class AutofacDispatcherRegistration
{
    public static void Register(ContainerBuilder builder, Assembly[] assemblies)
    {
        builder.Register(ctx =>
            {
                IComponentContext componentContext = ctx.Resolve<IComponentContext>();
                return new Dispatcher(new AutofacServiceProviderAdapter(componentContext));
            })
            .As<IDispatcher>()
            .InstancePerLifetimeScope();

        builder.RegisterAssemblyTypes(assemblies)
            .AsClosedTypesOf(typeof(IRequestHandler<,>))
            .InstancePerDependency();

        builder.RegisterAssemblyTypes(assemblies)
            .AsClosedTypesOf(typeof(IPipelineBehavior<,>))
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