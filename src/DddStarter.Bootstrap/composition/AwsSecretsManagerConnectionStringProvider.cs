using System.Collections.Concurrent;
using System.Text.Json;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using DddStarter.Application.Contracts.Ports;
using DddStarter.Infrastructure.Configuration;

namespace DddStarter.Bootstrap.Composition;

internal sealed class AwsSecretsManagerConnectionStringProvider : IConnectionStringProvider
{
    private readonly IAmazonSecretsManager _secretsManager;
    private readonly AppSettings _appSettings;
    private readonly ConcurrentDictionary<string, string> _cache = new(StringComparer.OrdinalIgnoreCase);

    public AwsSecretsManagerConnectionStringProvider(IAmazonSecretsManager secretsManager, AppSettings appSettings)
    {
        _secretsManager = secretsManager;
        _appSettings = appSettings;
    }

    public string GetRequiredConnectionString(string connectionName)
    {
        return _cache.GetOrAdd(connectionName, ResolveConnectionString);
    }

    private string ResolveConnectionString(string connectionName)
    {
        string secretId = GetSecretId(connectionName);
        GetSecretValueResponse response = _secretsManager.GetSecretValueAsync(new GetSecretValueRequest
        {
            SecretId = secretId
        }).GetAwaiter().GetResult();

        string? secretString = response.SecretString;
        if (string.IsNullOrWhiteSpace(secretString))
        {
            throw new InvalidOperationException($"AWS Secrets Manager secret '{secretId}' did not contain a SecretString value.");
        }

        return ExtractConnectionString(secretString, connectionName, secretId);
    }

    private string GetSecretId(string connectionName)
    {
        if (_appSettings.AwsSecretsManager.ConnectionSecretIds.TryGetValue(connectionName, out string? mappedSecretId) && !string.IsNullOrWhiteSpace(mappedSecretId))
        {
            return mappedSecretId;
        }

        string prefix = _appSettings.AwsSecretsManager.SecretIdPrefix;
        if (string.IsNullOrWhiteSpace(prefix))
        {
            throw new InvalidOperationException($"AppSettings:AwsSecretsManager could not resolve a secret id for connection '{connectionName}'.");
        }

        return string.Concat(prefix, connectionName);
    }

    private static string ExtractConnectionString(string secretString, string connectionName, string secretId)
    {
        string trimmed = secretString.Trim();
        if (!trimmed.StartsWith("{", StringComparison.Ordinal))
        {
            return trimmed;
        }

        using JsonDocument document = JsonDocument.Parse(trimmed);
        JsonElement root = document.RootElement;

        if (TryGetPropertyIgnoreCase(root, connectionName, out JsonElement namedValue) && namedValue.ValueKind == JsonValueKind.String)
        {
            return namedValue.GetString()!;
        }

        if (TryGetPropertyIgnoreCase(root, "connectionString", out JsonElement connectionStringValue) && connectionStringValue.ValueKind == JsonValueKind.String)
        {
            return connectionStringValue.GetString()!;
        }

        if (TryGetPropertyIgnoreCase(root, "value", out JsonElement valueElement) && valueElement.ValueKind == JsonValueKind.String)
        {
            return valueElement.GetString()!;
        }

        throw new InvalidOperationException($"AWS Secrets Manager secret '{secretId}' did not contain a string value for '{connectionName}', 'connectionString', or 'value'.");
    }

    private static bool TryGetPropertyIgnoreCase(JsonElement element, string propertyName, out JsonElement value)
    {
        foreach (JsonProperty property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }
}