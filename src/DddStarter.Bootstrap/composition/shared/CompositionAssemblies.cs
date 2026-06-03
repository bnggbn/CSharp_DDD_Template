using System.Reflection;
using DddStarter.Application;
using DddStarter.Controller;
using DddStarter.Domain;
using DddStarter.Infrastructure;

namespace DddStarter.Bootstrap.Composition;

/// <summary>
/// Exposes the assemblies that participate in bootstrap-time scanning and registration.
/// </summary>
internal static class CompositionAssemblies
{
    /// <summary>
    /// Gets the complete ordered assembly set used by the composition root.
    /// </summary>
    public static Assembly[] All =>
    [
        typeof(BootstrapAssemblyMarker).Assembly,
        typeof(ApplicationAssemblyMarker).Assembly,
        typeof(InfrastructureAssemblyMarker).Assembly,
        typeof(DomainAssemblyMarker).Assembly,
        typeof(ControllerAssemblyMarker).Assembly
    ];
}
