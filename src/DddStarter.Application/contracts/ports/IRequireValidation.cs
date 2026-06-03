using MediatR;

namespace DddStarter.Application.Contracts.Ports;

public interface IRequireValidation<out TResponse> : IRequest<TResponse>
{
}
