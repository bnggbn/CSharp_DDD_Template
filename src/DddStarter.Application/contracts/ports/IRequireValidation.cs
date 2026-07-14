using DddStarter.Dispatching.Contracts;

namespace DddStarter.Application.Contracts.Ports;

public interface IRequireValidation<out TResponse> : IRequest<TResponse>
{
}
