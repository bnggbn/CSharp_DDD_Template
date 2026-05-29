using System;

namespace DddStarter.Domain.ValueObjects;

public sealed class ExampleVo
{
    public ExampleVo(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value is required.", nameof(value));
        }

        Value = value.Trim();
    }

    public string Value { get; }
}
