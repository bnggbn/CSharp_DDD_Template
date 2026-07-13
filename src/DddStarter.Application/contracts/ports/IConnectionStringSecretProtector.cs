namespace DddStarter.Application.Contracts.Ports;

public sealed record ProtectedConnectionStringSecret(string SecretPath, string KeyDirectoryPath, bool ReusedExistingKeyRing);

public interface IConnectionStringSecretProtector
{
    ProtectedConnectionStringSecret Protect(string connectionName, string connectionString);
}