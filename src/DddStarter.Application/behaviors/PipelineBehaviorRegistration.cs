using DddStarter.Dispatching.Contracts;

namespace DddStarter.Application.Behaviors;

public static class PipelineBehaviorRegistration
{
    public static IEnumerable<Type> FindOpenGenericBehaviorTypes(IEnumerable<System.Reflection.Assembly> assemblies)
    {
        return assemblies
            .SelectMany(static assembly => assembly.GetTypes())
            .Where(static type => type is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: true })
            .Where(static type => type.GetInterfaces()
                .Any(static iface => iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>)))
            .Distinct();
    }
}
