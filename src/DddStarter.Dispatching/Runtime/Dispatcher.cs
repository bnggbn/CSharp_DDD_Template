using System.Reflection;
using DddStarter.Dispatching.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace DddStarter.Dispatching.Runtime;

public sealed class Dispatcher : IDispatcher
{
    private static readonly MethodInfo SendCoreMethod = typeof(Dispatcher)
        .GetMethod(nameof(SendCore), BindingFlags.Instance | BindingFlags.NonPublic)!;

    private readonly IServiceProvider _serviceProvider;

    public Dispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        object? result = SendCoreMethod
            .MakeGenericMethod(request.GetType(), typeof(TResponse))
            .Invoke(this, new object?[] { request, cancellationToken });

        return (Task<TResponse>)result!;
    }

    private Task<TResponse> SendCore<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken)
        where TRequest : IRequest<TResponse>
    {
        IRequestHandler<TRequest, TResponse> handler = _serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();
        IEnumerable<IPipelineBehavior<TRequest, TResponse>> behaviors = _serviceProvider.GetServices<IPipelineBehavior<TRequest, TResponse>>();

        RequestHandlerDelegate<TResponse> next = () => handler.Handle(request, cancellationToken);
        foreach (IPipelineBehavior<TRequest, TResponse> behavior in behaviors.Reverse())
        {
            RequestHandlerDelegate<TResponse> current = next;
            next = () => behavior.Handle(request, current, cancellationToken);
        }

        return next();
    }
}