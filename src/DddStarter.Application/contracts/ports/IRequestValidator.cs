using System.Collections.Generic;

namespace DddStarter.Application.Contracts.Ports;

public interface IRequestValidator<in TRequest>
{
    IEnumerable<string> Validate(TRequest request);
}
