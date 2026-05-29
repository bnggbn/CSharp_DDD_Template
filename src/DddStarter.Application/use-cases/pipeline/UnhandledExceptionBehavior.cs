using DddStarter.Application.Contracts.Ports;
using MediatR;

namespace DddStarter.Application.UseCases.Pipeline;

public sealed class UnhandledExceptionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IAppLogger _logger;

    public UnhandledExceptionBehavior(IAppLogger logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception exception)
        {
            _logger.Error($"Unhandled request exception for '{typeof(TRequest).Name}'.", exception);
            throw;
        }
    }
}
