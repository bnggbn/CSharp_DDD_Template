using System.Collections.Generic;

internal sealed class LinterPolicy
{
    public bool RequireAsciiPath { get; set; } = true;
    public BuildGateRule BuildGateRule { get; set; } = new();
    public List<LayerRule> DependencyRules { get; set; } = new();
    public List<NamingRule> NamingRules { get; set; } = new();
    public List<PathTypeRule> PathTypeRules { get; set; } = new();
    public FileNamingRule FileNamingRule { get; set; } = new();
    public List<MediatRScopeRule> MediatRScopeRules { get; set; } = new();
    public ConfigMutationRule ConfigMutationRule { get; set; } = new();
    public CqrsCommandFileRule CqrsCommandFileRule { get; set; } = new();
    public CqrsQueryFileRule CqrsQueryFileRule { get; set; } = new();
    public UseCaseFileRule UseCaseFileRule { get; set; } = new();
    public RequestImmutabilityRule RequestImmutabilityRule { get; set; } = new();
    public CqrsInheritanceRule CqrsInheritanceRule { get; set; } = new();
    public WorkflowConstructorRule WorkflowConstructorRule { get; set; } = new();
    public WorkflowDispatchRule WorkflowDispatchRule { get; set; } = new();
    public HandlerDispatchRule HandlerDispatchRule { get; set; } = new();
    public ControllerWorkflowRule ControllerWorkflowRule { get; set; } = new();
    public SeverityMutationRule SeverityMutationRule { get; set; } = new();
    public ConstructorInterfaceRule ConstructorInterfaceRule { get; set; } = new();
    public InterfaceMockRule InterfaceMockRule { get; set; } = new();
    public ConstantsClassRule ConstantsClassRule { get; set; } = new();
}

internal sealed class BuildGateRule
{
    public string RuleId { get; set; } = "BUILD001";
    public bool Enabled { get; set; } = true;
    public List<string> SolutionSearchPatterns { get; set; } = new() { "*.slnx", "*.sln" };
    public List<string> ExcludedProjectFileNames { get; set; } = new();
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
    public string CommandsPathContains { get; set; } = "/application/use-cases/";
}

internal sealed class CqrsQueryFileRule
{
    public string RuleId { get; set; } = "CQRS101";
    public string QueriesPathContains { get; set; } = "/application/use-cases/";
}

internal sealed class UseCaseFileRule
{
    public string RuleId { get; set; } = "CQRS102";
    public bool Enabled { get; set; } = true;
    public string UseCasesPathContains { get; set; } = "/application/use-cases/";
}

internal sealed class RequestImmutabilityRule
{
    public string RuleId { get; set; } = "IMM001";
    public bool Enabled { get; set; } = true;
    public string RequestsPathContains { get; set; } = "/application/use-cases/";
}

internal sealed class CqrsInheritanceRule
{
    public string RuleId { get; set; } = "CQRS103";
    public bool Enabled { get; set; } = true;
    public string RequestsPathContains { get; set; } = "/application/use-cases/";
    public List<string> AllowedInterfaceTypeNames { get; set; } = new();
}

internal sealed class WorkflowConstructorRule
{
    public string RuleId { get; set; } = "FLOW001";
    public bool Enabled { get; set; } = true;
    public string WorkflowsPathContains { get; set; } = "/application/workflows/";
    public List<string> AllowedDependencyTypeNames { get; set; } = new();
}

internal sealed class WorkflowDispatchRule
{
    public string RuleId { get; set; } = "FLOW002";
    public bool Enabled { get; set; } = true;
    public string WorkflowsPathContains { get; set; } = "/application/workflows/";
    public List<string> AllowedRequestSuffixes { get; set; } = new();
}

internal sealed class HandlerDispatchRule
{
    public string RuleId { get; set; } = "FLOW003";
    public bool Enabled { get; set; } = true;
    public string HandlersPathContains { get; set; } = "/application/handlers/";
    public List<string> ForbiddenDependencyTypeNames { get; set; } = new();
    public List<string> ForbiddenInvocationNames { get; set; } = new();
}

internal sealed class ControllerWorkflowRule
{
    public string RuleId { get; set; } = "CTRL001";
    public bool Enabled { get; set; } = true;
    public string ControllersPathContains { get; set; } = "/controller/";
    public List<string> AllowedDependencyTypeNames { get; set; } = new();
    public List<string> AllowedDependencySuffixes { get; set; } = new();
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
    public List<string> AllowedConcreteTypeSuffixes { get; set; } = new();
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
