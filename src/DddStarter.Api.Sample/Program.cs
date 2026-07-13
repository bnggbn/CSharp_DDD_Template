using DddStarter.Bootstrap;
using DddStarter.Bootstrap.Composition;

await using AppHost appHost = AppHost.Create(ControllerKind.Api);
return await appHost.RunAsync(args);