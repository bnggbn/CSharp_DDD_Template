using DddStarter.Application.Contracts.Ports;

namespace DddStarter.Bootstrap.Composition;

internal sealed class UnsupportedConnectionStringSecretProtector : IConnectionStringSecretProtector
{
    private readonly string _providerKind;

    public UnsupportedConnectionStringSecretProtector(string providerKind)
    {
        _providerKind = providerKind;
    }

    public ProtectedConnectionStringSecret Protect(string connectionName, string connectionString)
    {
        throw new NotSupportedException($"The configured connection string provider '{_providerKind}' does not support local secret protection.");
    }
}