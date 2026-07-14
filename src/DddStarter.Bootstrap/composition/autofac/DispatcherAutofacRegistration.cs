using System.Reflection;
using Autofac;
using DddStarter.Application.Contracts.Ports;
using DddStarter.Dispatching.Runtime;

namespace DddStarter.Bootstrap.Composition;

/// <summary>
/// Registers dispatcher services and validators into Autofac.
/// </summary>
internal static class DispatcherAutofacRegistration
{
    /// <summary>
    /// Registers dispatcher services into an Autofac container builder.
    /// </summary>
    /// <param name="builder">The target Autofac container builder.</param>
    /// <param name="assemblies">The assemblies to scan for dispatcher components.</param>
    public static void Register(ContainerBuilder builder, Assembly[] assemblies)
    {
        AutofacDispatcherRegistration.Register(builder, assemblies);
        builder.RegisterAssemblyTypes(assemblies)
            .AsClosedTypesOf(typeof(IRequestValidator<>))
            .AsImplementedInterfaces()
            .InstancePerDependency();
    }
}