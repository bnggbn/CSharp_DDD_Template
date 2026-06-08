using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

internal static class RegexRuleRunner
{
    public static void Run(LinterPolicy policy, List<string> files, List<string> issues)
    {
        CheckNamingRules(policy, files, issues);
        CheckPathTypeRules(policy, files, issues);
        CheckFileNamingRule(policy, files, issues);
        CheckMediatRScopeRules(policy, files, issues);
        CheckConfigMutationRules(policy, files, issues);
        CheckInterfaceMockRule(policy, files, issues);
        CheckAsciiPathRule(policy, files, issues);
    }

    private static void CheckNamingRules(LinterPolicy policy, List<string> files, List<string> issues)
    {
        foreach (NamingRule rule in policy.NamingRules)
        {
            Regex regex = new(rule.TypeRegex, RegexOptions.Compiled);
            foreach (string file in files)
            {
                string rel = Normalize(file);
                if (!PathMatchesScope(rel, rule.PathContains))
                {
                    continue;
                }

                string text = File.ReadAllText(file);
                foreach (Match match in regex.Matches(text))
                {
                    string typeName = match.Groups[1].Value;
                    bool suffixOk = string.IsNullOrEmpty(rule.RequiredSuffix) || typeName.EndsWith(rule.RequiredSuffix, StringComparison.Ordinal);
                    bool prefixOk = string.IsNullOrEmpty(rule.RequiredPrefix) || typeName.StartsWith(rule.RequiredPrefix, StringComparison.Ordinal);
                    if (!suffixOk || !prefixOk)
                    {
                        issues.Add($"[{rule.RuleId}] {rel}: type '{typeName}' violates naming policy.");
                    }
                }
            }
        }
    }

    private static void CheckPathTypeRules(LinterPolicy policy, List<string> files, List<string> issues)
    {
        foreach (PathTypeRule rule in policy.PathTypeRules)
        {
            Regex regex = new(rule.TypeRegex, RegexOptions.Compiled);
            foreach (string file in files)
            {
                string rel = Normalize(file);
                string text = File.ReadAllText(file);
                foreach (Match match in regex.Matches(text))
                {
                    string typeName = match.Groups[1].Value;
                    if (IsNestedTypeContainerFile(file, typeName))
                    {
                        continue;
                    }

                    if (!PathMatchesScope(rel, rule.RequiredPathContains))
                    {
                        issues.Add($"[{rule.RuleId}] {rel}: type '{typeName}' must be under '{rule.RequiredPathContains}'.");
                    }
                }
            }
        }
    }

    private static void CheckFileNamingRule(LinterPolicy policy, List<string> files, List<string> issues)
    {
        if (!policy.FileNamingRule.Enabled)
        {
            return;
        }

        Regex regex = new(@"\b(class|interface|record)\s+([A-Za-z0-9_]+)", RegexOptions.Compiled);
        foreach (string file in files)
        {
            string rel = Normalize(file);
            string text = File.ReadAllText(file);
            Match match = regex.Match(text);
            if (!match.Success)
            {
                continue;
            }

            string declaredType = match.Groups[2].Value;
            string fileName = Path.GetFileNameWithoutExtension(file);
            if (!string.Equals(fileName, declaredType, StringComparison.Ordinal) &&
                !fileName.StartsWith(declaredType + ".", StringComparison.Ordinal))
            {
                issues.Add($"[{policy.FileNamingRule.RuleId}] {rel}: file name should match primary type '{declaredType}'.");
            }
        }
    }

    private static void CheckMediatRScopeRules(LinterPolicy policy, List<string> files, List<string> issues)
    {
        foreach (MediatRScopeRule rule in policy.MediatRScopeRules)
        {
            foreach (string file in files)
            {
                string rel = Normalize(file);
                if (!PathMatchesScope(rel, rule.ForbiddenPathContains))
                {
                    continue;
                }

                string text = File.ReadAllText(file);
                if (text.Contains(rule.ForbiddenUsing, StringComparison.Ordinal))
                {
                    issues.Add($"[{rule.RuleId}] {rel}: MediatR usage is out of allowed scope.");
                }
            }
        }
    }

    private static void CheckConfigMutationRules(LinterPolicy policy, List<string> files, List<string> issues)
    {
        Regex assignmentRegex = new($@"\b[A-Za-z0-9_\.]+\s*\.\s*{Regex.Escape(policy.ConfigMutationRule.OverridePropertyName)}\s*=", RegexOptions.Compiled);
        Regex writeRegex = new($@"File\s*\.\s*WriteAllText\s*\([^\)]*{Regex.Escape(policy.ConfigMutationRule.SettingsFileName)}", RegexOptions.Compiled | RegexOptions.Singleline);

        foreach (string file in files)
        {
            string rel = Normalize(file);
            string text = File.ReadAllText(file);

            if (assignmentRegex.IsMatch(text) && !IsAllowed(rel, policy.ConfigMutationRule.AllowedAssignmentPathContains))
            {
                issues.Add($"[{policy.ConfigMutationRule.AssignmentRuleId}] {rel}: override assignment is restricted to approved paths.");
            }

            if (writeRegex.IsMatch(text) && !IsAllowed(rel, policy.ConfigMutationRule.AllowedSettingsWritePathContains))
            {
                issues.Add($"[{policy.ConfigMutationRule.FileWriteRuleId}] {rel}: settings file write is restricted to approved paths.");
            }
        }
    }

    private static void CheckInterfaceMockRule(LinterPolicy policy, List<string> files, List<string> issues)
    {
        if (!policy.InterfaceMockRule.Enabled)
        {
            return;
        }

        Regex ifaceRegex = new(@"\binterface\s+(I[A-Za-z0-9_]+)", RegexOptions.Compiled);
        List<string> searchScopeFiles = files.Where(file =>
        {
            string rel = Normalize(file);
            return policy.InterfaceMockRule.SearchPathContains.Any(path => PathMatchesScope(rel, path));
        }).ToList();

        foreach (string file in files)
        {
            string rel = Normalize(file);
            if (!policy.InterfaceMockRule.InterfacePathContains.Any(path => PathMatchesScope(rel, path)))
            {
                continue;
            }

            string text = File.ReadAllText(file);
            foreach (Match ifaceMatch in ifaceRegex.Matches(text))
            {
                string interfaceName = ifaceMatch.Groups[1].Value;
                string baseName = interfaceName.Length > 1 ? interfaceName.Substring(1) : interfaceName;

                bool foundMock = searchScopeFiles.Any(candidate =>
                {
                    string candidateText = File.ReadAllText(candidate);
                    if (candidateText.Contains(": " + interfaceName, StringComparison.Ordinal))
                    {
                        return true;
                    }

                    return policy.InterfaceMockRule.AllowedMockSuffixes.Any(suffix =>
                        candidateText.Contains("class " + baseName + suffix, StringComparison.Ordinal));
                });

                if (!foundMock)
                {
                    issues.Add($"[{policy.InterfaceMockRule.RuleId}] {rel}: interface '{interfaceName}' has no mock/fake implementation in allowed search paths.");
                }
            }
        }
    }

    private static bool IsAllowed(string rel, List<string> allowedPaths)
    {
        return allowedPaths.Any(path => PathMatchesScope(rel, path));
    }

    private static bool IsNestedTypeContainerFile(string file, string typeName)
    {
        string fileName = Path.GetFileNameWithoutExtension(file);
        return fileName.Contains('.', StringComparison.Ordinal) &&
               fileName.StartsWith(typeName + ".", StringComparison.Ordinal);
    }

    private static bool PathMatchesScope(string rel, string scope)
    {
        if (string.IsNullOrWhiteSpace(scope))
        {
            return false;
        }

        if (rel.Contains(scope, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        string normalizedScope = scope.Replace('\\', '/').Trim();
        string mappedScope = normalizedScope switch
        {
            var value when value.StartsWith("/application/", StringComparison.OrdinalIgnoreCase) => ".Application/" + value["/application/".Length..],
            var value when value.StartsWith("/domain/", StringComparison.OrdinalIgnoreCase) => ".Domain/" + value["/domain/".Length..],
            var value when value.StartsWith("/infrastructure/", StringComparison.OrdinalIgnoreCase) => ".Infrastructure/" + value["/infrastructure/".Length..],
            var value when value.StartsWith("/controller/", StringComparison.OrdinalIgnoreCase) => ".Controller/" + value["/controller/".Length..],
            var value when value.StartsWith("/bootstrap/", StringComparison.OrdinalIgnoreCase) => ".Bootstrap/" + value["/bootstrap/".Length..],
            _ => normalizedScope.TrimStart('/')
        };

        return rel.Contains(mappedScope, StringComparison.OrdinalIgnoreCase);
    }

    private static void CheckAsciiPathRule(LinterPolicy policy, List<string> files, List<string> issues)
    {
        if (!policy.RequireAsciiPath)
        {
            return;
        }

        foreach (string file in files)
        {
            string rel = Normalize(file);
            if (rel.Any(ch => ch > 127))
            {
                issues.Add($"[ASCII001] {rel}: path contains non-ASCII characters.");
            }
        }
    }

    private static string Normalize(string path)
    {
        return path.Replace('\\', '/');
    }
}
