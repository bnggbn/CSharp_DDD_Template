param(
    [string]$PolicyPath = "src/GenericDddLinter/linter.policy.sample.json",
    [string]$OutPath = "docs/LINT_RULES.generated.md"
)

$policy = Get-Content $PolicyPath -Raw | ConvertFrom-Json
$descriptions = @{}
if ($policy.ruleDescriptions) {
    $policy.ruleDescriptions.PSObject.Properties | ForEach-Object { $descriptions[$_.Name] = [string]$_.Value }
}

$ids = @()

if ($policy.requireAsciiPath -eq $true) { $ids += "ASCII001" }
if ($policy.buildGateRule.ruleId) { $ids += [string]$policy.buildGateRule.ruleId }

foreach ($r in @($policy.dependencyRules)) { if ($r.ruleId) { $ids += [string]$r.ruleId } }
foreach ($r in @($policy.namingRules)) { if ($r.ruleId) { $ids += [string]$r.ruleId } }
foreach ($r in @($policy.pathTypeRules)) { if ($r.ruleId) { $ids += [string]$r.ruleId } }
if ($policy.fileNamingRule.ruleId) { $ids += [string]$policy.fileNamingRule.ruleId }
foreach ($r in @($policy.mediatRScopeRules)) { if ($r.ruleId) { $ids += [string]$r.ruleId } }
if ($policy.configMutationRule.assignmentRuleId) { $ids += [string]$policy.configMutationRule.assignmentRuleId }
if ($policy.configMutationRule.fileWriteRuleId) { $ids += [string]$policy.configMutationRule.fileWriteRuleId }
if ($policy.cqrsCommandFileRule.ruleId) { $ids += [string]$policy.cqrsCommandFileRule.ruleId }
if ($policy.cqrsQueryFileRule.ruleId) { $ids += [string]$policy.cqrsQueryFileRule.ruleId }
if ($policy.useCaseFileRule.ruleId) { $ids += [string]$policy.useCaseFileRule.ruleId }
if ($policy.requestImmutabilityRule.ruleId) { $ids += [string]$policy.requestImmutabilityRule.ruleId }
if ($policy.cqrsInheritanceRule.ruleId) { $ids += [string]$policy.cqrsInheritanceRule.ruleId }
if ($policy.workflowConstructorRule.ruleId) { $ids += [string]$policy.workflowConstructorRule.ruleId }
if ($policy.workflowDispatchRule.ruleId) { $ids += [string]$policy.workflowDispatchRule.ruleId }
if ($policy.handlerDispatchRule.ruleId) { $ids += [string]$policy.handlerDispatchRule.ruleId }
if ($policy.controllerWorkflowRule.ruleId) { $ids += [string]$policy.controllerWorkflowRule.ruleId }
if ($policy.severityMutationRule.ruleId) { $ids += [string]$policy.severityMutationRule.ruleId }
if ($policy.constructorInterfaceRule.ruleId) { $ids += [string]$policy.constructorInterfaceRule.ruleId }
if ($policy.interfaceMockRule.ruleId) { $ids += [string]$policy.interfaceMockRule.ruleId }
if ($policy.constantsClassRule.ruleId) { $ids += [string]$policy.constantsClassRule.ruleId }

$sorted = $ids | Sort-Object -Unique

$lines = @()
$lines += "# Lint Rules (Generated)"
$lines += ""
$lines += "Generated from policy: $PolicyPath."
$lines += ""
$lines += "| RuleId | Description |"
$lines += "|---|---|"
foreach ($id in $sorted) {
    $desc = if ($descriptions.ContainsKey($id)) { $descriptions[$id] } else { "(no description in policy)" }
    $lines += "| $id | $desc |"
}

Set-Content -Path $OutPath -Value ($lines -join "`r`n")
Write-Host "Generated $OutPath with $($sorted.Count) rules."
