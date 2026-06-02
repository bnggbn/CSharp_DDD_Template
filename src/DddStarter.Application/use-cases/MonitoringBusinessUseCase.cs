using MediatR;

namespace DddStarter.Application.UseCases.Commands;

public static partial class MonitoringBusinessUseCase
{
    public sealed record ExecuteCommand(string TriggeredBy) : IRequest<Unit>;
    public sealed record DeleteCommand(string TriggeredBy, string TargetId) : IRequest<Unit>;
}
