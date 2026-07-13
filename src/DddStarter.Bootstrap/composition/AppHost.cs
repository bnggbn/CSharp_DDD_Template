using DddStarter.Bootstrap.Composition;
using DddStarter.Controller.Abstractions;
using DddStarter.Controller.Api;
using DddStarter.Controller.Cli;
using DddStarter.Controller.Console;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace DddStarter.Bootstrap;

/// <summary>
/// Owns the composition root: builds configuration, the service provider, and resolves the selected controller.
/// </summary>
public sealed class AppHost : IAsyncDisposable
{
    private readonly ServiceProvider _provider;
    private readonly IAppController _controller;

    private AppHost(ServiceProvider provider, IAppController controller)
    {
        _provider = provider;
        _controller = controller;
    }

    /// <summary>
    /// Builds the configured application host for the requested controller.
    /// </summary>
    /// <param name="controllerKind">The controller adapter to run.</param>
    /// <returns>A ready-to-run application host.</returns>
    public static AppHost Create(ControllerKind controllerKind = ControllerKind.Console)
    {
        IConfiguration configuration = BuildConfiguration();

        ServiceCollection services = new();
        services.AddDddStarter(configuration);

        ServiceProvider provider = services.BuildServiceProvider();
        IAppController controller = ResolveController(provider, controllerKind);

        return new AppHost(provider, controller);
    }

    /// <summary>
    /// Runs the selected controller.
    /// </summary>
    /// <param name="args">The process arguments.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>The controller's exit code.</returns>
    public Task<int> RunAsync(string[] args, CancellationToken cancellationToken = default)
    {
        return _controller.RunAsync(args, cancellationToken);
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        return _provider.DisposeAsync();
    }

    /// <summary>
    /// Builds the application configuration from <c>appsettings.json</c> and environment variables.
    /// </summary>
    /// <returns>The composed configuration source.</returns>
    private static IConfiguration BuildConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();
    }

    /// <summary>
    /// Resolves the controller adapter matching the requested kind.
    /// </summary>
    /// <param name="provider">The built service provider.</param>
    /// <param name="controllerKind">The controller adapter to resolve.</param>
    /// <returns>The resolved controller.</returns>
    private static IAppController ResolveController(IServiceProvider provider, ControllerKind controllerKind)
    {
        return controllerKind switch
        {
            ControllerKind.Console => provider.GetRequiredService<ConsoleController>(),
            ControllerKind.Cli => provider.GetRequiredService<CliController>(),
            ControllerKind.Api => provider.GetRequiredService<ApiController>(),
            _ => throw new ArgumentOutOfRangeException(nameof(controllerKind), controllerKind, "Unsupported controller kind.")
        };
    }
}
