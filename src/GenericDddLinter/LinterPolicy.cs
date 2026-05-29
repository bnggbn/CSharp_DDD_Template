using System.Collections.Generic;

internal sealed class LinterPolicy
{
    public bool RequireAsciiPath { get; set; } = true;
    public List<LayerRule> DependencyRules { get; set; } = new();
    public List<NamingRule> NamingRules { get; set; } = new();
    public List<PathTypeRule> PathTypeRules { get; set; } = new();
    public FileNamingRule FileNamingRule { get; set; } = new();
    public List<MediatRScopeRule> MediatRScopeRules { get; set; } = new();
    public ConfigMutationRule ConfigMutationRule { get; set; } = new();
    public CqrsCommandFileRule CqrsCommandFileRule { get; set; } = new();
    public CqrsQueryFileRule CqrsQueryFileRule { get; set; } = new();
    public SeverityMutationRule SeverityMutationRule { get; set; } = new();
    public ConstructorInterfaceRule ConstructorInterfaceRule { get; set; } = new();
    public InterfaceMockRule InterfaceMockRule { get; set; } = new();
    public ConstantsClassRule ConstantsClassRule { get; set; } = new();
}

internal sealed class LayerRule
{
    public string RuleId { get; set; } = "DEP000";
    public string PathContains { get; set; } = "";
    public List<string> ForbiddenNamespaces { get; set; } = new();
}

internal sealed class NamingRule
{
    public string RuleId { get; set; } = "NAME000";
    public string PathContains { get; set; } = "";
    public string TypeRegex { get; set; } = "";
    public string RequiredPrefix { get; set; } = "";
    public string RequiredSuffix { get; set; } = "";
}

internal sealed class PathTypeRule
{
    public string RuleId { get; set; } = "PATH000";
    public string TypeRegex { get; set; } = "";
    public string RequiredPathContains { get; set; } = "";
}

internal sealed class FileNamingRule
{
    public string RuleId { get; set; } = "FILE001";
    public bool Enabled { get; set; } = true;
}

internal sealed class MediatRScopeRule
{
    public string RuleId { get; set; } = "MEDIATR000";
    public string ForbiddenPathContains { get; set; } = "";
    public string ForbiddenUsing { get; set; } = "using MediatR";
}

internal sealed class ConfigMutationRule
{
    public string AssignmentRuleId { get; set; } = "CFG001";
    public string FileWriteRuleId { get; set; } = "CFG002";
    public string OverridePropertyName { get; set; } = "MonitorSeverityOverrides";
    public string SettingsFileName { get; set; } = "appsettings.json";
    public List<string> AllowedAssignmentPathContains { get; set; } = new();
    public List<string> AllowedSettingsWritePathContains { get; set; } = new();
}

internal sealed class CqrsCommandFileRule
{
    public string RuleId { get; set; } = "CQRS100";
    public string CommandsPathContains { get; set; } = "/application/use-cases/commands/";
}

internal sealed class CqrsQueryFileRule
{
    public string RuleId { get; set; } = "CQRS101";
    public string QueriesPathContains { get; set; } = "/application/use-cases/queries/";
}

internal sealed class SeverityMutationRule
{
    public string RuleId { get; set; } = "SEV001";
    public string TargetMethodName { get; set; } = "SetSeverity";
    public List<string> AllowedPathContains { get; set; } = new();
}

internal sealed class ConstructorInterfaceRule
{
    public string RuleId { get; set; } = "DIP001";
    public bool Enabled { get; set; } = true;
    public List<string> TargetPathContains { get; set; } = new();
}

internal sealed class InterfaceMockRule
{
    public string RuleId { get; set; } = "MOCK001";
    public bool Enabled { get; set; } = true;
    public List<string> InterfacePathContains { get; set; } = new();
    public List<string> SearchPathContains { get; set; } = new();
    public List<string> AllowedMockSuffixes { get; set; } = new();
}

internal sealed class ConstantsClassRule
{
    public string RuleId { get; set; } = "CONST001";
    public bool Enabled { get; set; } = true;
    public List<string> TargetPathContains { get; set; } = new();
    public string ClassSuffix { get; set; } = "Constants";
}
