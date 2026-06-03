using DddStarter.Application.Contracts.Ports;
using MediatR;

namespace DddStarter.Application.UseCases.Commands;

public static partial class MonitoringBusinessUseCase
{
    public sealed record ExecuteCommand(string TriggeredBy) : IRequireValidation<Unit>;
    public sealed record DeleteCommand(string TriggeredBy, string TargetId) : IRequest<Unit>;
}
