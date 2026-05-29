using System.Collections.Generic;
using System.Linq;
using DddStarter.Application.Contracts.Ports;
using MediatR;

namespace DddStarter.Application.UseCases.Pipeline;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IRequestValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IRequestValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        string[] errors = _validators
            .SelectMany(v => v.Validate(request))
            .Where(message => !string.IsNullOrWhiteSpace(message))
            .Distinct()
            .ToArray();

        if (errors.Length > 0)
        {
            throw new RequestValidationException(typeof(TRequest), errors);
        }

        return next();
    }
}
