using DddStarter.Application.Workflows;
using DddStarter.Application.Contracts.Ports;
using DddStarter.Controller.Abstractions;

namespace DddStarter.Controller.Console;

public sealed class ConsoleController : IAppController
{
    private readonly IConnectionStringSecretProtector _connectionStringSecretProtector;
    private readonly MonitoringWorkflow _workflow;

    public ConsoleController(MonitoringWorkflow workflow, IConnectionStringSecretProtector connectionStringSecretProtector)
    {
        _workflow = workflow;
        _connectionStringSecretProtector = connectionStringSecretProtector;
    }

    public async Task<int> RunAsync(string[] args, CancellationToken cancellationToken = default)
    {
        if (TryHandleSecretsProtect(args, out int exitCode))
        {
            return exitCode;
        }

        await _workflow.ExecuteAsync("ConsoleController", cancellationToken);
        return 0;
    }

    private bool TryHandleSecretsProtect(string[] args, out int exitCode)
    {
        exitCode = 0;

        if (args.Length == 0)
        {
            return false;
        }

        if (!string.Equals(args[0], "secrets", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (args.Length != 4 || !string.Equals(args[1], "protect", StringComparison.OrdinalIgnoreCase))
        {
            global::System.Console.Error.WriteLine("Usage: secrets protect <ConnectionName> <ConnectionString>");
            exitCode = 1;
            return true;
        }

        string connectionName = args[2];
        string connectionString = args[3];

        try
        {
            ProtectedConnectionStringSecret result = _connectionStringSecretProtector.Protect(connectionName, connectionString);
            string keyRingMessage = result.ReusedExistingKeyRing ? "existing key ring reused" : "new key ring created";
            global::System.Console.WriteLine($"Protected '{connectionName}' to '{result.SecretPath}'.");
            global::System.Console.WriteLine($"Key directory: '{result.KeyDirectoryPath}' ({keyRingMessage}).");
            exitCode = 0;
        }
        catch (Exception ex)
        {
            global::System.Console.Error.WriteLine(ex.Message);
            exitCode = 1;
        }

        return true;
    }
}
