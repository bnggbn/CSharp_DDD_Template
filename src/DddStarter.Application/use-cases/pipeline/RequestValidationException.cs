using System;
using System.Collections.Generic;
using System.Linq;

namespace DddStarter.Application.UseCases.Pipeline;

public sealed class RequestValidationException : Exception
{
    public RequestValidationException(Type requestType, IEnumerable<string> errors)
        : base(BuildMessage(requestType, errors))
    {
        Errors = errors.ToArray();
    }

    public IReadOnlyList<string> Errors { get; }

    private static string BuildMessage(Type requestType, IEnumerable<string> errors)
    {
        string joined = string.Join("; ", errors);
        return $"Validation failed for '{requestType.Name}': {joined}";
    }
}
