using MediatR;

namespace DddStarter.Application.UseCases.Commands;

public sealed record ExecuteMonitoringCommand(string TriggeredBy) : IRequest<Unit>;
